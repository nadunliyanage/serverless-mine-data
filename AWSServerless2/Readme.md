Instructions to deploy with the termial/command prompt.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Deploy application
```
    cd "AWSServerless2"
    dotnet lambda deploy-serverless
```
The Table name, Stack name and the bucket name would be as same as specified in the aws-lambda-tools-default.json

The table structure will be as defined in the MineData class.
Thus, the columns will be
    Guid - Holds the GUID
    CreatedTimestamp - Holds the created time of the entry