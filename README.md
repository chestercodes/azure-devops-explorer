# azure-devops-explorer

> warning: work in progress, not currently being run in production or used seriously anywhere. 

pulls down data and writes to sql, local db by default.

the data is stuff that i'm currently finding useful in my day role as a security engineer with some analysis and enrichment of the expanded template yamls

## data 

the following are currently retrieved:

- builds - gets all completed from start or latest from each pipeline
- - build artifacts
- - build timelines
- build expanded yaml - runs analysis to find used service connection and variable groups
- git pull requests
- git pull request reviewers
- identities
- pipeline
- pipeline runs - with repository and pipeline artifacts used
- pipeline checks
- pipeline environments
- secure files
- service endpoints
- service endpoints execution history
- security namespaces
- security namespaces actions
- security acls
- variable groups

## data future

might eventually try to archive

- full audit

## todo

- in import change code, remove items that are in DB but are no longer returned from api
- in import code, check where pagination is required
- import permissions somehow

## how to use

if not using localdb, need to change the values in the `DatabaseConnection` class.

this is a work in progress, haven't fully committed to creating the initial migration so you'll need to do this:

```
cd src/AzureDevopsExplorer.Database

dotnet ef migrations add InitialCreate
dotnet ef database update
```

once that is done can run by changing the values in the AzureDevopsExplorer.Console/Properties/localSettings.json file.

the actions can be seen in the AzureDevopsExplorer.Application/Configuration/DataAction.cs file

## Neo4j

The data can be loaded into a docker container of neo4j. To spin this up, run docker compose in the root of the project.

To make it load the latest entity values to neo4j use the `--load-to-neo4j` flag.