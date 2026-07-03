# README

## Required related services.

This template is pre-configured to work with the Identity Server, and Authorisation templates. 
You will need to create projects from these templates as well, with the same name used for this template.


## Db context and migrations

The template contains the initial migration with the models required for the solution to start up correctly.

Once you have added your domain models, it is recommended to delete the initial migration, and re-generate it. 
When ready follow the steps explained in the `Migrations` project -> `\Migrations\z_EFCommands.txt`.


## Reporting and notifications

The reporting, and notifications db contexts should not require any changes, or new migrations to be generated, 
unless the packages are out of date. 

By default, the reporting and notifications db context connection strings point to their own databases. 
This can be changed so that the domain, reporting, and notifications contexts point to the same database.