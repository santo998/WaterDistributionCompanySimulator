Water distribution company simulator:

A folder "..\Docs" is included, containing all documentation associated with this project:

- Architecture Diagram: Contains the distributed system architecture diagram.
- Design Justification: Contains some decisions, assumptions, and justifications in the system design.
- Demo1: Contains a demo of messaging functionality using the Telegram API via VS (Visual Studio).
- Demo2: Contains a demo of messaging functionality using the Telegram API via Docker CLI (command line interface).
    

To simplify user configuration, we modified the Rabbit image to include two users:

1) The local_jobs user, used for communication between nodes.

	Username = local_jobs
	Password = password123

2) The admin user, used for user administration from Rabbit management.

	Username = admin
	Password = admin

These users are configured in the Rabbit image.
Rabbit's administration environment is configured at the URL "localhost:8081".
Accessing with the admin user allows monitoring the system and performing CRUD operations on Rabbit users.
Also included is a secrets.json file containing Rabbit credentials and database connection. This file should not be included in version control (for security reasons), so it was included with Telegram API token empty.

When configuring in Visual Studio:
Right-click on NodoServidor → Manage user secrets.
Replace the content with that of the secrets.json file in the solution.

Telegram Bot:
We use a Telegram bot to send loss notifications to subscribed users.


Bot Creation:
To create the bot, follow these steps:

1) Go to the Telegram app.
2) Search for the "BotFather" contact.
3) If prompted with the "/start" command, send it.
4) Send the "/newbot" command, starting the bot creation.
5) Give it a name (e.g., "WaterDistributionCompany").
6) Assign a username ending in "Bot" (e.g., "WaterDistributionCompanyBot").
7) The bot is now created and ready for configuration.

Bot Configuration:
To configure the bot, follow these steps:

1) After bot creation, BotFather provides a token for HTTP API access. Copy and paste it into the secrets.json file under the "API_Telegram" label and as the value for the "Token".

    The directory of the secrets.json file is:
    If run in Visual Studio, its directory is: "%AppData%/Microsoft/UserSecrets/ec4e0d66-90d0-40bf-a4be-3ea3d138eb39/secrets.json".
    If run with the "Docker compose.bat" script, the secrets.json file is in the solution directory.

    Example: Assuming the token is "tokenExample1234", the secrets.json file would contain:

    "API_Telegram": {
   	"Token": "tokenExample1234"
    }

3) Search for the bot on Telegram and send it any message (e.g., "Hello").
    If prompted with the "/start" command, send it, to be able to write the initial message later.
    Our system, before notifying, updates subscriptions in the database with new messages the bot received. Then, it retrieves subscriptions from the database and sends a "broadcast" through our Telegram bot to the chats initiated by users.
    The advantage of updating subscriptions just before notifying is that users can subscribe to the bot at any time.
    If not writing to the bot, no notifications will be received. If there are no subscribers, the system will notify that it is necessary to write to the bot to receive notifications.
    Security could be added by having users send certain keywords to the bot. Currently, any message is valid, so anyone who knows our bot's username can subscribe and receive notifications about system losses.

To run the system, there are two ways:
The recommended one is using the "Docker compose.bat" script, which uses Docker CLI to run the docker-compose.

The procedure for this is as follows:
Open the "Docker compose.bat" file.
The command prompt will open, third-party images will be obtained from Docker Hub, our projects will be compiled, creating the corresponding images, and all will be unified in a compose.
In the terminal, you can see the outputs of the different containers.

The other way is using Visual Studio.
The procedure for this is as follows:

    1) Set the docker-compose project as the startup project.
    2) Run the solution.
    3) In the output, you can see messages from Nodo Servidor and Clients, but not third-party services like SQL Server and Rabbit, which do occur in the Docker Compose CLI.

Whether run through the script or Visual Studio, if Docker Desktop is available, each container can be accessed separately, and its respective output can be viewed.
Remember to follow the instructions for creating and configuring the Telegram bot. Otherwise, notifications will not be received.

Configuration:
	Environment variables:

	Client Node:
	"CON_PERDIDAS": Boolean variable indicating if the node will present losses in ALL its measurements.
	Optional. Default value = false, indicating that the node will NOT present losses in ANY of its measurements.
	
	"TOLERANCIA_MEDICIONES": Double variable indicating the tolerance in measurements, i.e., the error range.
	Optional. Default value = 0.005 L/s.
	
	Node data (used to register the node in the database):
	"ID_NODO": Integer variable indicating the node's ID.
	Mandatory. If not specified, the program cannot continue.
	
	"PADRE_NODO": Integer variable indicating the parent node's ID.
	Optional. Default value = null.
	
	"NOMBRE_NODO": String variable indicating the node's name.
	Mandatory. If not specified, the program cannot continue.
	
	"CAUDAL_ESPERADO": Integer variable indicating the expected flow of the node.
	Mandatory. If not specified, the program cannot continue.
	
	Server Node:
	"PERSISTE_NODOS_Y_MEDICIONES": Boolean variable indicating if the server will persist nodes and measurements received from Rabbit.
	Optional. Default value = true.
	
	"DETECTA_PERDIDAS": Boolean variable indicating if the server will search for losses among the measurements received from Rabbit.
	Optional. Default value = true.
	
	Node data:
	"NOMBRE_NODO": String variable indicating the node's name.
	Mandatory. If not specified, the program cannot continue.
