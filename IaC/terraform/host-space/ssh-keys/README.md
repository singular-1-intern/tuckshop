# Public SSH Keys folder
When you want to provision Ops VMs, the public SSH keys must be added to this folder. These are installed into the Ops VM when it is created, enabling Ansible to connect to the machine using the private key stored on your machine.

These keys should use the following naming scheme:
`id_rsa_{client_prefix}_{environment}`

See the README in the module root for detail on generating these SSH keys.