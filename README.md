# WATS Converter - Spea ICT 3030 format

A WATS Client converter plugin for importing test data from Spea ICT 3030 test system to WATS.

## Getting Started

* [About WATS](https://wats.com/manufacturing-intelligence/)
* [About submitting data to WATS](https://virinco.zendesk.com/hc/en-us/articles/207424613)
* [About creating custom converters](https://virinco.zendesk.com/hc/en-us/articles/207424593)

## Download

See the Custom Converter section in the [WATS Client Installation Guide](https://wats.com/download) for your version of the WATS Client for how to install a converter.

### Test-sofware configuration

This is an example converter for Spea ICT 3030 that only supports output in the same format as the example files. Different configurations of the test software will require an accordingly different converter.

### Parameters

This converter uses the following parameters:

| Parameter         | Default value         | Description                                                    |
|-------------------|-----------------------|----------------------------------------------------------------|
| operationTypeCode | 10                    | If log is missing operation code (process code), use this one. |
| operator  		| sysoper               | If log is missing operator use this one.						 |
| sequenceName		| SeqName               | If log is missing sequence name, use this one.                 |
| sequenceVersion	| 0.0.0                 | If log is missing sequence version, use this one.              |
| partNumber		| 10                    | If log is missing operation code (process code), use this one. |

## Testing

The project uses the [MSTest framework](https://docs.microsoft.com/en-us/visualstudio/test/quick-start-test-driven-development-with-test-explorer) for testing the converter.

It is setup with two tests; one for setting up the API by registering the client to your WATS, and one for running the converter.

The values are hardcoded in the test, so you will need to change the values to reflect your setup.
* In SetupClient, fill in your information in the the call to RegisterClient.
* In TestICT3030Converter, fill in the path to the file you want to test the converter with. There are example files in the Data folder.
* Run SetupClient once, then you can run TestICT3030Converter as many times as you want.

## Contributing

We're open to suggestions! Feel free open an issue or create a pull request.

Please read [Contributing](CONTRIBUTING.md) for details on contributions.

## Troubleshooting

#### Converter failed to start

Symptom:
* Converter did not start after being configured.

Possible cause:
* WATS Client Service does not have folder permission to the input path.
* WATS Client Service was not restarted after configuration.

Possible solution:
* [Give NETWORK SERVICE write permission to the input path folder](https://virinco.zendesk.com/hc/en-us/articles/207424113-WATS-Client-Add-write-permission-to-NETWORK-SERVICE-on-file-system-to-allow-converter-access)
* Make a change in a converter configuration and undo the change, click APPLY. When asked to restart the service, click Yes.

#### Converter class drop down list is empty

Symptom:
* The converter class drop down list in the Client configurator is empty after adding a converter DLL.

Possible cause:
* The DLL file is blocked. Windows blocks files that it thinks are untrusted, which stops them from being executed.

Possible solution:
* Open properties on the file and unblock it.

#### Other

Contact Virinco support, and include the wats.log file: [Where to find the wats log file at the Client](https://virinco.zendesk.com/hc/en-us/articles/207424033-Where-to-find-the-wats-log-file-at-the-Client).

## Contact

* Issues with the converter or suggestions for improvements can be submitted as an issue here on GitHub.
* Ask questions about WATS in the [WATS Community Help](https://virinco.zendesk.com/hc/en-us/community/topics/200229613)
* Suggestions for the WATS Client itself or WATS in general can be submitted to the [WATS Idea Exchange](https://virinco.zendesk.com/hc/en-us/community/topics/200229623)
* Sensitive installation issues or other sensitive questions can be sent to [support@virinco.com](mailto://support@virinco.com)

## License

This project is licensed under the [LGPLv3](COPYING.LESSER) which is an extention of the [GPLv3](COPYING).