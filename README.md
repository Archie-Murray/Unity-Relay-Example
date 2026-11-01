# Unity Relay Example

A basic example of the required networking for using Unity's Relay Service allowing clients to connect to a server without the need for port-fowarding

> [!NOTE]
> You may need to allow the server through any firewalls when prompted (Likely Windows specific) - on other platforms you may neeed to configure networking settings accordingly

## How to use

Run the server and client, and monitor the server console log for a message like this:
```console
[Lobby] Server Join Code: <6 Digit Join Code Here>
```
At this point the server has started and is ready to accept clients, paste this into the 'Enter Code' box and press the 'Start Client' button, if all goes well you should see the following screen:
![Successful Connect Client](./Share/Successful_Connect_Client.png)

## Next Steps
This is only an example for the absolute bare minimum, you may want to check connection requests via something like [Network Manager - Connection Approval](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/basics/connection-approval.html) where the payload `byte[]` can be used to store things like password hashes or usernames/ids so the server only needs to respond to valid connection requests, you may also want a 'lobby stage' where all players connect before the game starts.
