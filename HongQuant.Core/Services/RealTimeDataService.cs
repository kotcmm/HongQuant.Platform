using DynamicData;
using HongQuant.Analysis.RealTime;
using HongQuant.Core.Models;
using MessagePack;
using MessagePack.Resolvers;
using System.Net.WebSockets;
using Websocket.Client;

namespace HongQuant.Core.Services;
public class RealTimeDataService : IRealTimeDataService
{
    private readonly WebsocketClient _websocketClient;

    private readonly SourceCache<RealTimeDataSource, string> _realTimeDataSources = new(x=>x.Product);
    public ISourceCache<RealTimeDataSource, string> RealTimeDataSources => _realTimeDataSources;

    public RealTimeDataService()
    {
        var url = new Uri("ws://127.0.0.1:5650");

        _websocketClient = new WebsocketClient(url)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(30)
        };

        _websocketClient.ReconnectionHappened.Subscribe(ReconnectionHappened);
        _websocketClient.MessageReceived.Subscribe(OnMessageReceived);
    }

    private void OnMessageReceived(ResponseMessage message)
    {
        if (message.MessageType == WebSocketMessageType.Binary)
        {
            var packData = MessagePackSerializer.Deserialize<PackData>(message.Binary, ContractlessStandardResolver.Options);

            if (packData.Type == 1)//历史数据
            {
                OnHistory(packData);
            }
            else if (packData.Type == 2)//实时数据
            {
                OnRealTime(packData);
            }
        }
    }

    private void OnHistory(PackData packData)
    {
        var historyFileData = MessagePackSerializer.Deserialize<HistoryFileData>(packData.Data, ContractlessStandardResolver.Options);

        var realTimeDataSource = new RealTimeDataSource
        {
            Product = historyFileData.Product,
            Strategy = historyFileData.Strategy,
        };

        _realTimeDataSources.AddOrUpdate(realTimeDataSource);

        realTimeDataSource.OnHistoryData(historyFileData.Datas);
    }

    private void OnRealTime(PackData packData)
    {
        var realTimeDatas = MessagePackSerializer.Deserialize<List<RealTimeFileData>>(packData.Data, ContractlessStandardResolver.Options);

        foreach (var realTimeData in realTimeDatas)
        {
            if(string.IsNullOrEmpty(realTimeData.Value)) continue;

            var data = _realTimeDataSources.Lookup(realTimeData.Product);
            
            if (data.HasValue)
            {
                data.Value.OnRealTimeData(realTimeData.Value);
            }
        }
    }

    private void ReconnectionHappened(ReconnectionInfo info)
    {
        Console.WriteLine($"Reconnection happened, type: {info.Type}");

        if (info.Type == ReconnectionType.Initial)
        {
            _websocketClient.Send("1");
        }
    }

    public async Task Start()
    {
        _realTimeDataSources.Clear();

        await _websocketClient.Start();
    }
}
