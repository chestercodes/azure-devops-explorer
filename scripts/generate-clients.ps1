# https://openapi-generator.tech/docs/generators/csharp

function GenerateClient ($schemaUrl, $packageName){
    docker run --rm `
        -v "$PSScriptRoot/..`:/local" openapitools/openapi-generator-cli generate `
        -i $schemaUrl `
        -g csharp `
        -o "/local/vsts-rest-api-specs/$packageName" `
        "--additional-properties=packageName=$packageName,targetFramework=net7.0,packageVersion=7.0"
    #    --additional-properties=packageName=VstsRestApiSpecs.Build,targetFramework=net7.0,packageVersion=7.0,library=httpclient

    cp -R "$PSScriptRoot/../vsts-rest-api-specs/$packageName/src/$packageName" "$PSScriptRoot/../src/$packageName"
}


function GenerateSql ($schemaUrl, $packageName){
    docker run --rm `
        -v "$PSScriptRoot/..`:/local" openapitools/openapi-generator-cli generate `
        -i $schemaUrl `
        -g mysql-schema `
        -o "/local/vsts-rest-api-specs/$packageName" `
        --additional-properties=packageName=VstsRestApiSpecs.Build,targetFramework=net7.0,packageVersion=7.0,library=httpclient

    cp -R "$PSScriptRoot/../vsts-rest-api-specs/$packageName/src/$packageName" "$PSScriptRoot/../src/$packageName"
}

# GenerateClient `
#     "https://raw.githubusercontent.com/MicrosoftDocs/vsts-rest-api-specs/master/specification/build/7.0/build.json" `
#     "VstsRestApiSpecs.Build"

GenerateSql `
    "https://raw.githubusercontent.com/MicrosoftDocs/vsts-rest-api-specs/master/specification/build/7.0/build.json" `
    "VstsRestApiSpecs.Build.Sql"