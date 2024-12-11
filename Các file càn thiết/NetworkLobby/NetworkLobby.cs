using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class NetworkLobby : MonoBehaviour
{
    [SerializeField] private RelayServices relayServices;
    [SerializeField] private NetworkPlayerInformation networkPlayer;
    NetworkLobbyUI networkLobbyUI;
    private Lobby hostLobby;
    public Lobby joinLobby;
    private string playerId;
    private float heartBeatTimer = 15,lobbyUpdateTimer;
    
    public string playerName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkLobbyUI = GetComponent<NetworkLobbyUI>();
    }

    void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
    }

    public async Task AuthenticationId(string name)
    {
        
        await UnityServices.InitializeAsync();
        
        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        playerId = AuthenticationService.Instance.PlayerId;
        playerName = name;
        networkPlayer.GetPlayerName(playerName);

    }
    private async void HandleLobbyHeartBeat()
    {
        if(joinLobby == null && IsHost() == false) return; 
        heartBeatTimer -= Time.deltaTime;
        if(heartBeatTimer < 0)
        {
            float heartBeatTimerMax = 15;
            heartBeatTimer = heartBeatTimerMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(joinLobby.Id);
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if(joinLobby == null) return;
        lobbyUpdateTimer -= Time.deltaTime;
        if(lobbyUpdateTimer < 0)
        {
            float lobbyUpdateTimerMax = 3f;
            lobbyUpdateTimer = lobbyUpdateTimerMax;

            await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);

            if(joinLobby.Data["KEY_START_GAME"].Value != "0")
            {
                if(!IsHost())
                {
                    await relayServices.JoinCode(joinLobby.Data["KEY_START_GAME"].Value);
                }
                joinLobby = null;
                return; 
            }
            networkLobbyUI.VisualizePlayerInfo();
        }
    }

    
    private bool IsHost()
    {
        if(joinLobby != null && joinLobby.HostId == playerId)
        {
            return true;
        }
        return false;
    }
    
    
    public async Task CreateLobby(string lobbyName,int maxPlayer, bool isPrivate)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    {"MAX_PLAYER", new DataObject(DataObject.VisibilityOptions.Member,Convert.ToString(maxPlayer))}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayer,createLobbyOptions);
            joinLobby = lobby;            
        }catch(LobbyServiceException e){Debug.LogError(e);}
    }


    public async Task Respone()
    {
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        Debug.Log(queryResponse.Results.Count);
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse  = await LobbyService.Instance.QueryLobbiesAsync();
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }catch(LobbyServiceException e){Debug.LogError(e);}

    }
    
    public async Task JoinLobbyByCode(string joinCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode,joinLobbyByCodeOptions);
            joinLobby = lobby;
        }catch(LobbyServiceException e){Debug.LogError(e);}
        
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }catch(LobbyServiceException e){Debug.LogError(e);}
    }
    
    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        return new Unity.Services.Lobbies.Models.Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
                };
    }
    private void PrintPlayer()
    {
        PrintPlayer(joinLobby);
    }
    private void PrintPlayer(Lobby lobby)
    {
        Debug.Log("Player in lobby" + lobby.Name + " " + lobby.Data["GameMode"].Value);
        foreach(Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id,new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            } );
            joinLobby = hostLobby;

            PrintPlayer(hostLobby);
        }catch(LobbyServiceException e){Debug.LogError(e);}
        
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
        }catch(LobbyServiceException e){Debug.LogError(e);}
        
    }

    public async Task LeaveRoom()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId);
            joinLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id,joinLobby.Players[1].Id);

        }catch (LobbyServiceException e){Debug.LogError(e);}
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id,new UpdateLobbyOptions
            {
                HostId = joinLobby.Players[1].Id
            } );
            joinLobby = hostLobby;

            PrintPlayer(hostLobby);
        }catch(LobbyServiceException e){Debug.LogError(e);}
    }

    private void DeleteLobby()
    {
        try
        {
            LobbyService.Instance.DeleteLobbyAsync(joinLobby.Id);

        }catch(LobbyServiceException e){Debug.LogError(e);}
    }
    public async Task JoinLobbyOnList(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            joinLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId,options);
        }catch(LobbyServiceException e){Debug.LogError(e);}
    }

    public async Task StartGame()
    {
        if(joinLobby.HostId != playerId) return;
        try
        {

            string relayCode = await relayServices.CreateRelay(int.Parse(joinLobby.Data["MAX_PLAYER"].Value));
            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinLobby.Id,new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member,relayCode)}
                }
            });
            joinLobby = lobby;
        }catch(LobbyServiceException e){Debug.LogError(e);}
    }
    private async void OnApplicationQuit()
    {
        if (joinLobby != null)
        {
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId);
        }
    }
}
