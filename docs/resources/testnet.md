# How to join the testnet

There’s two ways to run a AElf node: you can either use Docker (recommended method) or run the binaries available on Github. Before you jump into the guides and tutorials you'll need to install the following tools and frameworks. For most of these dependencies we provide ready-to-use command line instructions. In case of problems or if you have more complex needs, we provide more information in the [Environment setup](../tutorials/setup/setup.md) section of this GitBook.

Summary of the steps to set up a node:

1. Execute the snapshot download script and load the snapshot into the database. 
2. Download our template setting files and docker run script. 
3. Modify the appsettings according to your needs. 
4. Run and check the node. 

## Setup the database

We currently support two key-value databases to store our nodes data: Redis and SSDB, but for the testnet we only provide snapshots for SSDB. We will configure two SSDB instances, one for chain database and one for the state database (run these on different machines for better performances).

### Import the snapshot data

After you’ve finished setting up the database, download the latest snapshots. The following gives you the template for the download URL,but you have to specify the snapshot date. We recommend you get the latest. 

Restore the chain database from snapshot:
```bash
>> mkdir snapshot
>> cd snapshot

## fetch the snapshot download script
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-mainchain-db.sh

## execute the script, you can optionally specify a date by appending “yyyymmdd” as parameter
>> sh download-mainchain-db.sh

## chain database: decompress and load the chain database snapshot
>> tar xvzf aelf-testnet-mainchain-chaindb-*.tar.gz
>> stop your chain database instance (ssdb server)
>> cp -r aelf-testnet-mainchain-chaindb-*/* /path/to/install/chaindb/ssdb/var/
>> start your chain database instance
>> enter ssdb console (ssdb-cli) to verify the imported data

## state database : decompress and load the state database
>> tar xvzf aelf-testnet-mainchain-statedb-*.tar.gz
>> stop your state database instance (ssdb server)
>> cp -r aelf-testnet-mainchain-statedb-*/* /path/to/install/ssdb/var/
>> start your state database instance
>> enter ssdb console(ssdb-cli) to verify the imported data
```

## Node configuration

### Generating the nodes account

This section explains how to generate an account for the node. First you need to install the aelf-command npm package. Open a terminal and enter the following command to install aelf-command:

```bash
>> npm i -g aelf-command
```

After installing the package, you can use the following command to create an account/key-pair:
```bash
>> aelf-command create
```

The command prompts for a password, enter it and don't forget it. The output of the command should look something like this:

```
AElf [Info]: Your wallet info is :
AElf [Info]: Mnemonic            : term jar tourist monitor melody tourist catch sad ankle disagree great adult
AElf [Info]: Private Key         : 34192c729751bd6ac0a5f18926d74255112464b471aec499064d5d1e5b8ff3ce
AElf [Info]: Public Key          : 04904e51a944ab13b031cb4fead8caa6c027b09661dc5550ee258ef5c5e78d949b1082636dc8e27f20bc427b25b99a1cadac483fae35dd6410f347096d65c80402
AElf [Info]: Address             : 29KM437eJRRuTfvhsB8QAsyVvi8mmyN9Wqqame6TsJhrqXbeWd
? Save account info into a file? Yes
? Enter a password: *********
? Confirm password: *********
✔ Account info has been saved to "/usr/local/share/aelf/keys/29KM437eJRRuTfvhsB8QAsyVvi8mmyN9Wqqame6TsJhrqXbeWd.json"
```

In the next steps of the tutorial you will need the Public Key and the Address for the account you just created. You'll notice the last line of the commands output will show you the path to the newly created key. The aelf directory is the data directory (datadir) and this is where the node will read the keys from.

Note that a more detailed section about the CLI can be found [here](cli/introduction.md).

### Prepare node configuration

```bash
## download the settings template and docker script
>> cd /tmp/ && wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-mainchain.zip
>> unzip aelf-testnet-mainchain.zip
>> mv aelf-testnet-mainchain /opt/aelf-node
```

Update the appsetting.json file with your account. This will require the information printed during the creation of the account. Open the appsettings.json file and edit the following sections.

The account/key-pair associated with the node we are going to run:
```json
"Account": {
    "NodeAccount": "2Ue31YTuB5Szy7cnr3SCEGU2gtGi5uMQBYarYUR5oGin1sys6H",
    "NodeAccountPassword": "********"
},
```

You also have to configure the database connection strings (port/db number):
```json
"ConnectionStrings": {
    "BlockchainDb": "ssdb://your chain database server ip address:port",
    "StateDb": "ssdb://your state database server ip address:port"
  },
```

Next add the testnet mainchain nodes as peer (bootnode peers):
```json
"Network": {
    "BootNodes": [
        "3.25.10.185:6800",
        "18.228.140.143:6800"
    ],
    "ListeningPort": 6800,
    "NetAllowed": "",
    "NetWhitelist": []
},
```

Note: if your infrastructure is behind a firewall you need to open the P2P listening port of the node.
You also need to configure your listening ip and port for the side chain connections:

```json
"CrossChain": {
    "Grpc": {
        "LocalServerPort": 5000,
        "LocalServerHost": "your server ip address",
        "ListeningHost": "0.0.0.0"
    }
},
```

## Running a full node with Docker

To run the node with Docker, enter the following commands:
```bash
## pull AElf’s image and navigate to the template folder to execute the start script
>> docker pull aelf/node:testnet-v0.8.2
>> cd /opt/aelf-node
>> sh aelf-node.sh start aelf/node:testnet-v0.8.2
```

to stop the node you can run:
```bash
>> sh aelf-node.sh stop
```

## Running a full node with the binary release

Most of AElf is developed with dotnet core, so to run the binaries you will need to download and install the .NET Core SDK before you start: [Download .NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0). For now AElf depends on version 3.0 of the SDK, on the provided link find the download for your platform, and install it.

Get the latest release with the following commands:
```bash
>> cd /tmp/ && wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-v0.8.2.zip
>> unzip aelf-v0.8.2.zip
>> mv aelf-v0.8.2 /opt/aelf-node/
```

Enter the configuration folder and run the node:
```bash
>> cd /opt/aelf-node
>> dotnet aelf-v0.8.2/AElf.Launcher.dll
```

## Check the node

You now should have a node that's running, to check this run the following command that will query the node for its current block height:

```bash
aelf-command get-blk-height -e http://127.0.0.1:8000
```

## Run side-chains

This section explains how to set up a sidechain node, you will have to repeat these steps for all side chains, essentially following these steps for each side-chain (currently five):

1. Fetch the appsettings and the docker run script. 
2. Download and restore the snapshot data with the URLs provided below (steps are the same as in A - Setup the database). 
3. Run the sidechain node. 

Running a side chain is very much like running a mainchain node, only configuration will change. Here you can find the instructions for sidechain1:
```bash
>> cd /tmp/ && wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain1.zip
>> unzip aelf-testnet-sidechain1.zip
>> mv aelf-testnet-sidechain1 /opt/aelf-node
```

In order for a sidechain to connect to a mainchain node you need to modify the configuration with the remote information.

```json
"CrossChain": {
    "Grpc": {
        "RemoteParentChainServerPort": 5000,
        "LocalServerHost": "you local ip address",
        "LocalServerPort": 5001,
        "RemoteParentChainServerHost": "your mainchain ip address",
        "ListeningHost": "0.0.0.0"
    },
    "ParentChainId": "AELF"
},
```

Here you can find the snapshot data for each sidechain, optionally you can specify the date, but we recommend you get the latest:

```
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-sidechain1-db.sh 
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-sidechain2-db.sh 
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-sidechain3-db.sh 
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-sidechain4-db.sh 
>> curl -O -s https://aelf-node.s3-ap-southeast-1.amazonaws.com/snapshot/testnet/download-sidechain5-db.sh
```

Here you can find the list of templates folders (appsettings and docker run script) for each side-chain:
```
wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain1.zip
wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain2.zip
wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain3.zip
wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain4.zip
wget https://github.com/AElfProject/AElf/releases/download/v0.8.2/aelf-testnet-sidechain5.zip

```

Each side chain has its own P2P network, you can find here some bootnodes that are available:
```
sidechain1 bootnode → ["13.211.28.67:6800", "18.229.184.199:6800"]
sidechain2 bootnode → ["13.236.40.223:6800", "18.229.191.226:6800"]
sidechain3 bootnode → ["13.239.50.175:6800", "18.229.195.182:6800"]
sidechain4 bootnode → ["13.55.199.121:6800", "18.229.233.20:6800"]
sidechain5 bootnode → ["3.104.42.91:6800", "52.67.206.106:6800"]
```

```json
"Network": {
    "BootNodes": [
        "Add the right boot node according sidechain"
    ],
    "ListeningPort": 6800,
    "NetAllowed": "",
    "NetWhitelist": []
},
```