using UnityEngine;
using System;

using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using Debug = UnityEngine.Debug;

public class TwitchListener : MonoBehaviour
{
    const string bot_access_token = "<bot-access-token-here>";

    Client client;
    PubSub sub;

    void Start()
    {
        var connectionCredentials = new ConnectionCredentials("<username-here>", bot_access_token);
        client = new Client();
        client.Initialize(connectionCredentials, "<username-here>");
        client.Connect();
        client.OnConnected += OnConnected;
        client.OnConnectionError += ClientOnOnConnectionError;
        client.OnError += ClientOnOnError;
        client.OnNoPermissionError += ClientOnOnNoPermissionError;
        client.OnSelfRaidError += ClientOnOnSelfRaidError;
    }

    void ClientOnOnSelfRaidError(object sender, EventArgs e)
    {
        Debug.LogError("Self raid error!");
    }

    void ClientOnOnNoPermissionError(object sender, EventArgs e)
    {
        Debug.LogError("No permission!");
    }

    void ClientOnOnError(object sender, OnErrorEventArgs e)
    {
        Debug.LogError($"Client error: {e.Exception}");
    }

    void ClientOnOnConnectionError(object sender, OnConnectionErrorArgs e)
    {
        Debug.LogError("Connection Error!");
    }

    void OnConnected(object obj, OnConnectedArgs args)
    {
        const string channelId = "<channel-id-here>";
        sub = new PubSub();
        sub.OnPubSubServiceConnected += OnPubSubConnected;
        sub.OnBitsReceived += OnBitsReceived;
        sub.OnChannelSubscription += OnSubscription;
        sub.OnFollow += SubOnOnFollow;
        sub.OnListenResponse += OnListenResponse;
        sub.OnPubSubServiceError += SubOnOnPubSubServiceError;
        sub.OnPubSubServiceClosed += SubOnOnPubSubServiceClosed;
        sub.ListenToBitsEvents(channelId);
        sub.ListenToSubscriptions(channelId);
        sub.ListenToFollows(channelId);
        sub.Connect();
    }

    void SubOnOnPubSubServiceClosed(object sender, EventArgs e)
    {
        Debug.LogError("PubSub closed.");
    }

    void SubOnOnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
    {
        Debug.LogError($"Pub sub error: {e.Exception}");
    }

    void OnListenResponse(object obj, OnListenResponseArgs args)
    {
        if(!args.Successful)
            Debug.LogError($"Error: {args.Response.Error} {args.Topic} {args.Response.Nonce}");
        else Debug.Log($"Now listening for bits! {args.Topic}");
    }
    
    void OnPubSubConnected(object obj, EventArgs args)
    {
        Debug.Log("Pub sub connected.");
        sub.SendTopics(bot_access_token);
    }

    void OnBitsReceived(object obj, OnBitsReceivedArgs args)
    {
        Debug.Log("Bits received!");
        var script = GetComponent<ManagerScript>();
        script.AddBits(args.BitsUsed);
    }

    void OnSubscription(object obj, OnChannelSubscriptionArgs args)
    {
        Debug.LogError("Subscription received!");
        var script = GetComponent<ManagerScript>();
        script.NewSubscriber();
    }
    
    void SubOnOnFollow(object sender, OnFollowArgs e)
    {
        Debug.LogError("Follower received!");
        var script = GetComponent<ManagerScript>();
        script.NewFollower();
    }
}
