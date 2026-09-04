# Clip06 - Reading the Fluent Configuration
Verify what we told EF Core actually got registered in the model.

## Demo Class
- DemonstrateFluentConfiguration.HighlightFluentConfigurationAsync

## What To Verify
- Owned type mapping, owned column names, shadow FK, and backing field are surfaced.
- Model metadata output matches current mapping.

## Tests
No code changes are made in this clip; no new tests apply.

## Student Changes
1. Make no new code edits in this clip
	- Action: This is a walkthrough/verification clip only.

2. Confirm required prior changes are already in place
	- Clip02 change applied in OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
	- Clip05 changes applied in OrderManagement.Domain/Aggregates/Order.cs and OrderConfiguration.cs

3. Run the fluent-configuration demo
	- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateFluentConfiguration.cs
	- Method: HighlightFluentConfigurationAsync
