# Tuckshop

This project uses yarn v2 as the package manager. Plug and play is enabled, and modules are installed globally.

- Ensure you have node version >= 18 installed.

## Install yarn (*Only if you have not done so before)
- Install yarn: `npm install -g yarn`
  - If you get an error about the script not being digitally signed, run this command: `Set-ExecutionPolicy ByPass`
- If you have node version 16, run `corepack enable`, otherwise run `npm i -g corepack`

## Install dependencies (*Do this for every new neo-react project)

- Run `yarn set version stable` to update your newly created package to the latest version of yarn.
- Run `yarn install` to install packages, they are not installed by default.
- Run `yarn dedupe` if you have build errors like x is not assignable to x.

## Yarn tools (*Do this for every new neo-react project)

For the project to work correctly under vscode, you will need to install the vscode sdk:

`yarn dlx @yarnpkg/sdks vscode`

- After installing this, vscode should prompt if it can use the workspace version of typescript.
- Click allow
- If you don't see a prompt, open a .ts file and click the `{}` icon next to typescript in the bottom right corner of vs code, and select the workspace typescript version.

It is also recommended to install the ZipFS vscode extension to be able to F12 into the zipped packages:
https://marketplace.visualstudio.com/items?itemName=arcanis.vscode-zipfs

## Certificate (*Only if you have not done so before)

The react dev server is configured to use the neo localhost certificate (configured in the `.env` file). You will need to install this certificate into your root certificate store. The script to do this can be found in the Certificates folder under the api project if you created a modular app from the neo .net templates. Otherwise you can find the certificates and install script here: https://github.com/SingularSystems/neo-templates/tree/master/Source/ModularApp/NeoTemplate.Core.Api/Certificates

## Running the project

* Run `yarn install` when pulling the repo, or when packages have been updated.

> **Do not** run `npm install`. If you have a `package_lock.json` file in the project root, you have done something wrong. Delete this file, and node_modules and re-run `yarn install`.

* To start in development mode, run `yarn start`.
* To build a production version, run `yarn build`. The compiled output will go into the `dist` folder.

If you look at the `.yarnrc.yml` file, you will see the `enableGlobalCache` yarn option is enabled. This means that packages are stored in a central location on disk, not in `node_modules`. This is why we use `yarn` to run the scripts in `package.json` instead of using `npm`.

## Debugging

After running `yarn start`, open the app in your browser in debug mode, by pressing <kbd>F5</kbd>.

To enable debugging your app in vs-code, add the following under ".vscode/launch.json":

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "type": "chrome",
            "request": "launch",
            "name": "Launch Chrome against localhost",
            "url": "https://localhost:3000",
            "webRoot": "${workspaceFolder}"
        }
    ]
}
```

## Demo Pages

The demo pages for neo components are no longer part of the template.

You can view the demo pages here: https://singularwebsites.co.za/reactdemo/

You can also clone the documentation repo and run the demo site yourself: https://github.com/SingularSystems/neo-project-example