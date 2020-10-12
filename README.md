# jv-api-poc
Journal Voucher API/Processing Proof of Concept

# Deps
### dotnet-svcutil
Generates clients and types from wsdl files.
```powershell 
dotnet tool install --global dotnet-svcutil
```
There is a bug in code generation in dotnet-svcutil where properties/fields with jagged arrays have 1 too many bracket pairs. The `gen.ps1` script provides an automated workaround, you must be in a project directory to run it.

##### Example
```powershell
cd JV.Lib.Integrations
..\gen.ps1 -wsdl https://wd5-impl-services1.workday.com/ccx/service/uw11/Integrations/v35.0/?wsdl
```

# Secrets
`.\Driver\appsettings.secrets.json` looks like:

```javascript
{
	"WorkdayCredentials": {
		"Username": "username",
		"Password": "password"
	}
}
```