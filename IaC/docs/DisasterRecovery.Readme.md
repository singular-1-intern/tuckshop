# Disaster Recovery Configuration & Failover Overview

This document provides an overview of the Disaster Recovery configuration and failover process for client hosted applications within CALM environments.

The `operationalPipelines` section of your `blueprint.json` file contains the main configuration for DR and other operational pipelines. You will have access to a default set of operational pipelines that should require minimal to no extra configuration, depending on your application's requirements.


## Client Configuration - Outline

The operational pipelines are configured with acceptable defaults out of the box. The various data restore and transfer pipelines can be configured using the `app-space/tfvars` and `host-space/tfvars` module files located in the `IaC/terraform` directory.

For `app-space`'s take note of the following configuration directives:

1. `storage_accounts`:
   - You can control the `Clear` directive with the `clear_operations` config block.
   - You can control the `Copy` directive with the `copy_operations` config block.
1. `sql_server`:
   - `restore_settings` is used to configure the DB restore process.
     - `excluded_databases` controls what databases to exclude from restoration.
     - `overwrite_existing` allows you to allow or deny the overwriting of existing databases.

The client's individual application configuration files located in the `IaC/config/apps` directory should also be updated.

For `application` configs take note of the following configuration directives:

1. App configuration files are located in `IaC/config/apps/{application}/{application}.{env}.yaml`.
1. Application configs are templated by default during a `Build-NeoBlueprint` script run.

## Disaster Recovery Failover Process - Outline

In the event of a disaster, the failover process is initiated primarily by running various operational pipelines. These pipelines need to be triggered in sequence. The DR failover process will be spearheaded by Singular's DevOps team in tandem with the respective client team(s).

The DR failover process is as follows:

1. Start the `DR AKS Cluster`, associated `Shared Host Resources` and clear the cluster - **This should only be done ONCE across the entire DR fail over process**.

   - Trigger the `sc_dr_ne_start` pipeline. _{DEVOPS}_
   - Trigger the `sc_dr_ne_shared_hosts_active` pipeline. _{DEVOPS}_
   - Trigger the `sc_dr_ne_clear_clusters` pipeline. _{DEVOPS}_

1. Provision the client project's spaces.

   - Trigger the `{project}_provision_app_space_dr` pipeline. _{PROJECT}{DEVOPS}_
   - Trigger the `{project}_provision_host_space_dr` pipeline. _{PROJECT}{DEVOPS}_

1. Restore the client application(s) data.

   - Trigger the `{project}_copy_key_vault_secrets_prd_to_dr` pipeline first. _{PROJECT}{DEVOPS}_
   - Trigger the `{project}_restore_databases_prd_to_dr` pipeline. _{PROJECT}{DEVOPS}_
   - Trigger the `{project}_copy_storage_accounts_prd_to_dr` pipeline. _{PROJECT}{DEVOPS}_

1. Deploy the client apps to the DR AKS Cluster.

   - Trigger all application (`{project}_deploy_ci_dr_{application}`) pipelines. _{PROJECT}{DEVOPS}_

---

> LEGEND: The below legend is used to denote the responsibility of the team member(s) who can/should trigger the respective pipeline(s).
>
> {DEVOPS} Steps marked as such must be triggered by a DevOps member.
>
> {PROJECT} Steps marked as such can be triggered by a Project member.

