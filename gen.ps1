[CmdletBinding()]
param (
    [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $false)]
    [System.String]
    $wsdl
)

function Resolve-Namespace() {
    $loc = Get-Location | Get-Item
    return $loc.Name
}

function Resolve-Csproj() {
    $loc = Get-Location
    $files = Get-ChildItem $loc -File

    foreach ($f in $files) {
        if ($f.Name.EndsWith(".csproj")) {
            return
        }   
    }

    throw "gen.ps1 must be executed in a project directory"
}

function Resolve-DotnetTools() {
    $dts = "${home}\.dotnet\tools\dotnet-svcutil.exe"
    if (!(Test-Path $dts)) {
        throw 'dotnet-svcutil.exe not found, install with "dotnet tool install --global dotnet-svcutil"' 
    }
}

class Correction {
    [int]$Line
    [string]$Content
}

class XmlArrayAttr {
    [int]$Line
    [int]$Brackets
}

function Test-Class([string]$line) {
    return $line.StartsWith("public partial class")
}

function Get-BracketCount([string]$content) {
    return [regex]::Matches($content, "\[\]" ).Count
}

function Resolve-MemberType([string]$line) {
    $parts = $line.Split()
    $len = $parts.Count
    return $parts[$len - 2]
}

function Test-BackingField([string]$type, [string]$line) {
    return $line.TrimStart().StartsWith("private") -and $line.Contains($type) -and $line.EndsWith("Field;")
}

function Test-XmlArrayAttr([string]$line) {
    return $line.TrimStart().StartsWith("[System.Xml.Serialization.XmlArrayItemAttribute(")
}

function Test-Attr([string]$line) {
    $strip = $line.Trim()
    return $strip.StartsWith("[") -and $strip.EndsWith("]")
}

function Test-Typeof([string]$line) {
    return $line.Contains("typeof(")
}

function Get-XmlArrayAttr([int]$num, [Collections.Generic.List[Correction]]$correct) {
    [XmlArrayAttr]$attr = $null
    $last = $false

    foreach ($c in [System.Linq.Enumerable]::Reverse($correct)) {
        if (Test-XmlArrayAttr $c.Content) {
            $last = $true
            if ($null -eq $attr) {
                $attr = [XmlArrayAttr]@{
                    Line = $c.Line
                    Brackets = Get-BracketCount $c.Content
                }
                continue
            }

            $attr = [XmlArrayAttr]@{
                Line = $c.Line
                Brackets = Get-BracketCount $c.Content
            }
            continue
        }

        if (($null -ne $attr) -and ($last -eq $true)) {
            return $attr
        }

        if (!(Test-Attr $c.Content)) {
            throw "Rewinding didn't locate a System.Xml.Serialization.XmlArrayItemAttribute. Began rewind on line: $num"
        }
    }

    throw "Unreachable"
}

function Get-BackingField([int]$num, [string]$type, [Collections.Generic.List[Correction]]$correct) {
    foreach ($c in [System.Linq.Enumerable]::Reverse($correct)) {
        if (Test-BackingField -type $type -line $c.Content) {
            return $c
        }

        if (Test-Class $c.Content) {
            throw "Rewinding didn't locate a backing field. Began rewind on line: $num"
        }
    }

    throw "Unreachable"
}

function Test-XmlTypeAttr([string]$content) {
    return $content.Trim().StartsWith("[System.Xml.Serialization.XmlTypeAttribute(");
}

function Test-ErrorClass([string]$class, [string]$og) {
    return $og.Trim() -eq "public partial class $class"
}

function Get-XmlTypeAttr([int]$num, [Collections.Generic.List[Correction]]$correct) {
    foreach ($c in [System.Linq.Enumerable]::Reverse($correct)) {
        if (Test-XmlTypeAttr -content $c.Content) {
            return $c
        }

        if (!(Test-Attr $c.Content)) {
            throw "Rewinding didn't locate a System.Xml.Serialization.XmlTypeAttribute. Began rewind on line: $num"
        }
    }
}

function Get-CorrectedXmlTypeAttr([string]$name, [string]$content) {
    if ($content.Contains("TypeName = ""$name""")) {
        return $content
    }

    return $content.Replace(")]", ", TypeName = ""$name"")]")
}

function Test-JaggedProperty([string]$og) {
    $trim = $og.Trim()
    return $trim.StartsWith("public") -and $trim.Contains("[][]")
}

function Get-CorrectedMember([int]$brackets, [string]$og) {
    $parts = $og.Split()
    $len = $parts.Count
    $type = $parts[$len - 2]
    $name = $type.TrimEnd("[]")

    $type = $name + ("[]" * $brackets)
    $parts[$len - 2] = $type

    $result = $parts | Join-String -Separator " "
    return $result
}

class Summary {
    [int]$Corrections
    [Collections.Generic.List[Correction]]$Content
}

function Get-CorrectionSummary([System.IO.FileInfo]$file) {
    $correct = [Collections.Generic.List[Correction]]::new()
    $fixes = 0
    foreach ($og in [System.IO.File]::ReadLines($file)) {
        $num++

        # we've landed on jagged array property
        if (Test-JaggedProperty $og) {
            [string]$proptype = Resolve-MemberType $og
            [XmlArrayAttr]$attr = Get-XmlArrayAttr -num $num -correct $correct
            $should = $attr.Brackets + 1
            [Correction]$field = Get-BackingField -num $num -type $proptype -correct $correct

            $field.Content = Get-CorrectedMember -brackets $should -og $field.Content
            $fixes++

            $prop = [Correction]@{
                Line = $num
                Content = (Get-CorrectedMember -brackets $should -og $og)
            }
            $fixes++
            $correct.Add($prop)
            continue
        }

        # we've landed on the Validation_ErrorType class
        if (Test-ErrorClass -class "Validation_ErrorType" -og $og) {
            [Correction]$xmlTypeAttr = Get-XmlTypeAttr -num $num -correct $correct
            $xmlTypeAttr.Content = Get-CorrectedXmlTypeAttr -name "Validation_Error" $xmlTypeAttr.Content
            $fixes++
        }

        # capture the unaltered line
        $cxn = [Correction]@{
            Line = $num
            Content = $og
        }
        $correct.Add($cxn)
    }

    return [Summary]@{
        Corrections = $fixes
        Content = $correct
    }
}

$folder = Resolve-Namespace

Resolve-Csproj

Resolve-DotnetTools

# make sure there is no ServiceReference folder, if there is, blow it away so we can regenerate
if (Test-Path .\ServiceReference) {
    Write-Host "Removing previous ServiceReference folder"
    Remove-Item .\ServiceReference -Recurse -Force
}

# check for success on this call, bail if error
dotnet-svcutil.exe $wsdl --serializer XmlSerializer --outputFile Reference.cs --namespace "*, $folder"

# dotnet-svcutil borked
if (!$?) {
    return
}

$generated = Join-Path .\ServiceReference Reference.cs | Get-Item

if (!(Test-Path $generated)) {
    throw [System.IO.FileNotFoundException] "Target file $generated not found"
}

[Summary]$summary = Get-CorrectionSummary $generated

$content = $summary.Content | Select-Object -Property Content | Join-String -Property Content -Separator "`n"
$corrections = $summary.Corrections

Set-Content -Path $generated -Value $content

Write-Host "Made $corrections corrections"
