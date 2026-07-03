## Ansible
This module uses ansible to configure the Operations Linux VM(s). An Ansible control node can only be executed on Linux, which then requires this terraform module to be executed from within an Ubuntu WSL session.

If you don't need to update the VMs, you can disable Ansible by setting the `ansible` variable's `enabled` property to `false` in the relevant `tfvars` file, i.e:
```tf
ansible = {
  enabled              = false
  private_ssh_key      = "~/.ssh/id_rsa_{client_prefix}_stg" # Key must be installed in current user's .ssh folder
  file_cleanup_enabled = false
}
```

## Ubuntu WSL Setup
The ansible runtime only works on Linux, so we need to set up an Ubuntu WSL installation with the required cli tools.

### Install Ubuntu WSL
- Enable `WSL 2+` on your Windows machine ([Install Guide](https://github.com/SingularSystems/singular-devops/blob/main/docs/docker/singular-devops-wsl2-developer-setup-guide.md))
- Open the Windows Store and search for `Ubuntu` and install it. Make sure you save the root password that is set during the initial setup. (You can find various articles on this basic installation online)
- Install Windows Terminal: https://apps.microsoft.com/store/detail/windows-terminal/9N0DX20HK701

### Tools Installation
- Launch `Windows Terminal`, On the Terminal window, click `+`, then select `Ubuntu (WSL)`
- Install Ansible: https://docs.ansible.com/ansible/latest/installation_guide/installation_distros.html#installing-ansible-on-ubuntu
- Install PowerShell: https://learn.microsoft.com/en-us/powershell/scripting/install/install-ubuntu?view=powershell-7.4
  (Version 7.4.x currently recommended. At time of writing, 7.5.0 has compatibility issues with Azure CLI)
- Install Terraform
  - Use the `Ubuntu/Debian` guide:
    - https://developer.hashicorp.com/terraform/tutorials/azure-get-started/install-cli
    - Under the 'Install Terraform' header, select the `Linux` tab, then `Ubuntu/Debian`
  - Once you've added the terraform repository, installing a specific version is straight forward:
    ```bash
    sudo apt-get update
    sudo apt-get install terraform=1.3.6
    ```
- Install Azure CLI: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt
- Install kubectl: Installation can be facilitated by AzureCLI by running `az aks install-cli`
- Install kubelogin: Download the latest 64-bit Linux binary here: `https://github.com/Azure/kubelogin/releases`. This must then be unzipped and copied into the machine's `/usr/bin` folder.

### Install SSH Keys
A public/private SSH key pair is required for each Ops VM provisioned. Typically only a single Ops VM is provisioned in a host space shared by multiple app spaces, so `stg` and `prd` key pairs should be sufficient. These keys should follow the naming convention `id_rsa_{client_prefix}_{environment}`.

The key pair can be created like this:
```bash
ssh-keygen -t rsa -b 4096 -C "devops@singular.co.za" -f id_rsa_sc_dev
```

In this case, the command would generate `id_rsa_sc_dev` (Private key) and `id_rsa_sc_dev.pub` (Public key) files. The `.pub` file should be placed in the host-space's `ssh-keys` folder.

The key pairs should then be copied to both your window's user `.ssh` folder, but also to your Ubuntu WSL instance's `.ssh` folder in the your user's `home` directory. This is to enable authentication with the Ops VM when running ansible. The sample below shows how to copy over a pair of private/public keys from a folder on your local windows machine (The recommendation is to initially copy them to your windows user's SSH folder). Note how more restrictive permissions are set on the private key (Read only for owner):
```bash
cd ~
mkdir .ssh
cd .ssh
cp /mnt/c/Users/{YourUser}/.ssh/id_rsa_{ProjectPrefix}_stg .
cp /mnt/c/Users/{YourUser}/.ssh/id_rsa_{ProjectPrefix}_stg.pub .
cp /mnt/c/Users/{YourUser}/.ssh/id_rsa_{ProjectPrefix}_prd .
cp /mnt/c/Users/{YourUser}/.ssh/id_rsa_{ProjectPrefix}_prd.pub .
chmod 400 ./id_rsa_{ProjectPrefix}_stg
chmod 400 ./id_rsa_{ProjectPrefix}_prd
```

In your `tfvars` file, the keys need to be referenced in two places
- The `ops_vm` object's `public_ssh_key` property should reference the location of the public key file in the host-space module.
- The `ansible` variable's `private_ssh_key` property should reference the home directory location of the private key.

*NOTE*: The ssh keys can be found in the `SingularCloudPasswords` vault under `Projects/{Project Name}/Server - Ops VM - SSH Key - Staging/Production`. (See the file attachments). The password on the entry is what you will need for the value of `TF_VAR_ansible_ssh_passphrase` in the following section.

## Running under Ubuntu WSL
To run this module under Ubuntu WSL, do the following:
- Launch an `Ubuntu (WSL)` session  in `Windows Terminal`
- Run `pwsh` to enter a PowerShell session
- Run `az login` and authenticate with Azure
- Navigate to the `shared-hosts` terraform module folder
- If Ansible Provisioning is enabled, ensure you set the SSH key passphrase environment variable: `$Env:TF_VAR_ansible_ssh_passphrase = "..."`
- Use `run.ps1` as usual (Note that you may need to run the upgrade action first to initialise the providers)

Example:
  ```bash
  pwsh
  az login
  cd /mnt/d/Clients/{RepoName}/IaC/terraform/host-space

  $Env:TF_VAR_ansible_ssh_passphrase = "..."

  ./run.ps1 we stg upgrade
  ./run.ps1 we stg
  ```

It is also suggested that you leave Ansible disabled on the initial run, so you can confirm that everything works for a standard run before adding Ansible into the mix.

## Troubleshooting
If you see alerts coming through regarding tunnel or proxy connectivity, there are a number of things you can check.

### Connect to the Ops VM
In order to connect to one of the Ops VMs, execute appropriate `ssh` command for the staging or production Ops VM:
```bash
# Connect to the Ops VM:
ssh -l {Username} {OpsVmName}
```

### Warp CLI Connection Status
Run the following command to check if WARP CLI for Linux is connected to the Cloudflare network:
```bash
warp-cli --accept-tos status
```

You should expect to see ```Status update: Connected```. If the connection is down, you can try to re-establish it by running:
```bash
warp-cli --accept-tos connect
```

If the client still fails to connect, proceed to debugging the WARP connection. Some things to check:
- Verify that the Cloudflare Service Token is still present/active in the Cloudflare Portal. You can check credentials in the `/var/lib/cloudflare-warp/mdm.xml` file on the Ops VM.
- Check the Cloudflare logs to see if there is a reason why the service account is failing to connect.

### Check if Nginx is running
Check if the Nginx instance is running:
```bash
systemctl status nginx
```

If the service is stopped, or showing an error, you can try the restart command:
```bash
systemctl restart nginx
```

You can also check the logs with these commands:
```bash
tail /var/log/nginx/error.log -n 30
tail /var/log/nginx/access.log -n 30
```

### Test URLs
To check if the Nginx proxy is correctly forwarding traffic to a remote service, run this command:
```bash
curl -H "neoauth: {NeoAuthPassword}" https://{OpsVmName}/uat/{ApiPath}
```
You can get the value for `neoauth` from the host-space Key Vault. Replace `{NeoAuthPassword}` in the command above with the actual value, `{OpsVmName}` with the name of the Ops VM you are testing against, and `{ApiPath}` with a valid endpoint on the remote API (E.g. a health check endpoint)

You can also test these calls from one of the application pods. If you are testing from any pod other than `Domain`, you will need to add the `-k` switch to the `curl` command to skip certificate validation. This is necessary because the proxy uses a self-signed certificate, and only the `Domain` pod is configured to trust the certificate.

You can also try make calls directly to the remove server, bypassing the proxy. This confirms if the Cloudflare tunnel is correctly forwarding traffic, E.g:
```bash
curl https://{RemoteServerNameOrIp}/{ApiPath}
```

### Nginx Configuration
If there is a problem with the nginx configuration, you can edit the file by running:
```bash
sudo nano /etc/nginx/sites-available/nginx-proxy.conf
```

You will need to restart the nginx service for any changes to take effect: `systemctl restart nginx`.

Note that if you make any changes to the nginx configuration, for them to be permanent, you will also need to update the templates in the `host-space` module accordingly. See `templates/nginx.conf` and `templates/nginx-location.conf`. Depending on what you have configured, you may also want to make it something that is configurable in the terraform variables.

