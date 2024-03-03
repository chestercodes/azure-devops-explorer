using AzureDevopsExplorer.Application;
using AzureDevopsExplorer.Application.Domain;

namespace AzureDevopsExplorer.UnitTests;

public class ParseYamlFileTests
{
    [Fact]
    public void ParsesYamlFile()
    {
        var yamlFileContent = @"
name: $(Date:yyyyMMdd)$(Rev:.r)
trigger:
  enabled: false
parameters:
- name: do_something
  type: boolean
  default: false
variables:
- name: all_env_var
  value: 'all_env_var_val'
- group: my-vg-1
stages:
- stage: pre
  displayName: ""Pre deployment stage""
  dependsOn: []
  jobs:
  - job: validation
    environment:
      name: my-env-1
    pool:
      vmImage: ubuntu-22.04
    variables:
    - name: release_name
      value: 'my_release'
    - group: my-vg-1
    steps:
    - task: PowerShell@2
      displayName: pre stage task
      condition: >-
        or (
          True,
          False,
          False
        )
      inputs:
        targetType: inline
        azureSubscription: my-sc-1
        script: >
          Write-Host ""pre stage task""
        pwsh: true
        errorActionPreference: stop
- stage: plan
  displayName: ""plan stage""
  dependsOn:
  - pre
  variables:
  - group: my-vg-2
  - group: my-vg-3
  - name: my-var-1
    value: my-var-1-val
  jobs:
  - job: run_plan
    variables:
    - name: environment
      value: production
    - group: my-vg-1
    - group: my-vg-4
    dependsOn: []
    condition: succeeded()
    pool:
      name: linux_pool
    environment:
      name: my-env-2
    steps:
    - task: PowerShell@2
      displayName: run plan task 1
      condition: always()
      inputs:
        targetType: inline
        connectedServiceNameARM: my-sc-2
        script: >
          Write-Host ""plan task 1""
        pwsh: true
        errorActionPreference: stop

    - task: AzureCLI@2
      displayName: run plan task 2
      enabled: False
      condition: and(succeeded(), not(False))
      inputs:
        addSpnToEnvironment: true
        azureSubscription: my-sc-3
        scriptType: pscore
        scriptLocation: inlineScript
        powerShellIgnoreLASTEXITCODE: false
        inlineScript: >
          Write-Host ""plan task 2""

  - job: post_plan
    variables:
    - name: environment
      value: production
    - group: my-vg-2
    - group: my-sc-1
    dependsOn: []
    condition: succeeded()
    pool:
      vmImage: ubuntu-22.04
    steps:
    - task: AzureCLI@2
      displayName: run post plan task 1
      enabled: False
      condition: and(succeeded(), not(False))
      inputs:
        addSpnToEnvironment: true
        azureSubscription: my-sc-4
        scriptType: pscore
        scriptLocation: inlineScript
        powerShellIgnoreLASTEXITCODE: false
        inlineScript: >
          Write-Host ""post plan task 1""

- stage: apply
  displayName: ""apply stage""
  dependsOn:
  - plan
  variables:
  - group: my-vg-4
  - name: my-var-2
    value: my-var-2-val
  jobs:
  - job: pre_apply
    pool:
      name: server
    timeoutInMinutes: 0
    variables:
    - name: environment
      value: production
    - group: my-vg-3
    - group: my-vg-4
    steps:
    - task: InvokeRESTAPI@1
      displayName: notify something
      enabled: false
      inputs:
        connectionType: connectedServiceName
        serviceConnection: my-sc-5
        method: POST
        body: >
          {
            """"thing1"""": """"thing2""""
          }
        waitForCompletion: false
  - deployment: apply
    variables:
    - name: my-var-3
      value: my-var-3-val
    - group: my-vg-5
    - group: my-vg-6
    timeoutInMinutes: 0
    environment:
      name: production
    strategy:
      runOnce:
        deploy:
          steps:
          - task: PowerShell@2
            displayName: apply task 1
            condition: always()
            inputs:
              targetType: inline
              script: >
                Write-Host ""apply task 1""
              pwsh: true
              errorActionPreference: stop

          - task: AzureCLI@2
            displayName: run apply task 2
            enabled: False
            condition: and(succeeded(), not(False))
            inputs:
              addSpnToEnvironment: true
              azureSubscription: my-sc-2
              scriptType: pscore
              scriptLocation: inlineScript
              powerShellIgnoreLASTEXITCODE: false
              inlineScript: >
                Write-Host ""post plan task 2""
";

        var parser = new ParseYamlFile();

        var result = parser.ParseExpandedBuildYaml(yamlFileContent);

        var pause = 0;
    }
}