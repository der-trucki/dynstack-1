using System;
using System.IO;
using System.Threading.Tasks;
using DynStack.DataModel.HS;
using DynStack.Simulation.HS;
using NetMQ;
using ProtoBuf;

namespace DynStack.SimulationRunner.HS {
  public class SimulationHost : Common.SimulationHost {
    private HotstorageSimulation sim;
    private bool aborted = false;

    public SimulationHost() { }

    protected override async Task<bool> RunSimulationAsync(byte[] settingsBuf, bool withPolicy = false) {
      var settings = Serializer.Deserialize<Settings>(settingsBuf.AsSpan());
      if (withPolicy)
        sim = new HotstorageSimulation(settings, new RuleBasedCranePolicy());
      else sim = new HotstorageSimulation(settings);
      sim.SetLogger(Logger);
      sim.WorldChanged += OnWorldChanged;

      await sim.RunAsync();
      Logger.WriteLine("Run completed");
      return !aborted;
    }

    protected override Task StopAsync() {
      sim.StopAsync();
      aborted = true;
      return Task.CompletedTask;
    }

    protected override async Task OnCraneMessageReceived(byte[] payload) {
      await Task.Delay(200);
      CraneSchedule schedule = null;
      try {
        schedule = Serializer.Deserialize<CraneSchedule>(payload.AsSpan());
      } catch (Exception ex) {
        Logger.WriteLine(ex.ToString());
      }
      if (schedule == null) return;
      await sim.SetCraneScheduleAsync(schedule);
    }

    private void OnWorldChanged(object sender, EventArgs e) {
      PublishWorldState(((HotstorageSimulation)sender).GetEstimatedWorldState());
    }

    private void PublishWorldState(World world) {
      using (var stream = new MemoryStream()) {
        Serializer.Serialize(stream, world);
        var bytes = stream.ToArray();
        var msg = new NetMQMessage();
        msg.AppendEmptyFrame();
        msg.Append("world");
        msg.Append(bytes);
        Outgoing.Enqueue(msg);
      }
    }

    protected override byte[] GetDefaultSettings(string setting) {
      using (var stream = new MemoryStream()) {
        var settings = DefaultSettings;

        switch (setting)
        {
          case "settings_213A":
            settings = settings_213A;
            break;
          case "settings_213C":
            settings = settings_213C;
            break;
          case "settings_216A":
            settings = settings_216A;
            break;
          case "settings_216C":
            settings = settings_216C;
            break;
          case "settings_23AE":
            settings = settings_23AE;
            break;
          case "settings_23AM":
            settings = settings_23AM;
            break;
          case "settings_23BE":
            settings = settings_23BE;
            break;
          case "settings_23BM":
            settings = settings_23BM;
            break;
          default:
            throw new NotImplementedException();
            break;
        }

        Serializer.Serialize(stream, settings);
        return stream.ToArray();
      }
    }

    protected override bool RunSimulation(byte[] settingsBuf, string url, string id, bool simulateAsync = true, bool useIntegratedPolicy = false) {
      var settings = Serializer.Deserialize<Settings>(settingsBuf.AsSpan());
      if (useIntegratedPolicy)
        sim = new HotstorageSimulation(settings, new RuleBasedCranePolicy());
      else
        sim = new HotstorageSimulation(settings, new SynchronousSimRunnerPolicy(url, id));

      sim.SetLogger(Logger);
      sim.SimulateAsync = simulateAsync;
      sim.WorldChanged += OnWorldChanged;
      Logger.WriteLine("Starting sim");
      sim.Run();

      return !aborted;
    }

    protected override byte[] GetDefaultSettings()
    {
      throw new NotImplementedException();
    }

    public static Settings DefaultSettings
    {
      get => new Settings()
      {
        // 21-3A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 3,
        CraneMoveTimeMean = TimeSpan.FromSeconds(1.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.3),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.25),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.05),
        DueTimeMean = TimeSpan.FromSeconds(232.8),
        DueTimeStd = TimeSpan.FromSeconds(42),
        ArrivalTimeMean = TimeSpan.FromSeconds(12.14),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.42),
        InitialNumberOfBlocks = 12,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }

    public static Settings settings_213A
    {
      get => new Settings()
      {
        // 21-3A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 3,
        CraneMoveTimeMean = TimeSpan.FromSeconds(1.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.3),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.25),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.05),
        DueTimeMean = TimeSpan.FromSeconds(232.8),
        DueTimeStd = TimeSpan.FromSeconds(42),
        ArrivalTimeMean = TimeSpan.FromSeconds(12.14),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.42),
        InitialNumberOfBlocks = 12,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_213C
    {
      get => new Settings()
      {
        // 21-3C
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 3,
        CraneMoveTimeMean = TimeSpan.FromSeconds(1.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.3),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.25),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.05),
        DueTimeMean = TimeSpan.FromSeconds(120.6),
        DueTimeStd = TimeSpan.FromSeconds(24),
        ArrivalTimeMean = TimeSpan.FromSeconds(9.44),
        ArrivalTimeStd = TimeSpan.FromSeconds(1.88),
        InitialNumberOfBlocks = 9,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,


        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_216A
    {
      get => new Settings()
      {
        // 21-6A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.4),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.5),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.1),
        DueTimeMean = TimeSpan.FromSeconds(480),
        DueTimeStd = TimeSpan.FromSeconds(96),
        ArrivalTimeMean = TimeSpan.FromSeconds(14.28),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.85),
        InitialNumberOfBlocks = 21,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,


        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_216C
    {
      get => new Settings()
      {

        // 21-6C
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.4),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.5),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.1),
        DueTimeMean = TimeSpan.FromSeconds(267),
        DueTimeStd = TimeSpan.FromSeconds(52.8),
        ArrivalTimeMean = TimeSpan.FromSeconds(11.11),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.22),
        InitialNumberOfBlocks = 18,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_23AE
    {
      get => new Settings()
      {
        // 23-AE
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(4),
        CraneMoveTimeStd = TimeSpan.FromSeconds(1),
        HoistMoveTimeMean = TimeSpan.FromSeconds(2),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.5),
        DueTimeMean = TimeSpan.FromSeconds(1602.6),
        DueTimeStd = TimeSpan.FromSeconds(300),
        ArrivalTimeMean = TimeSpan.FromSeconds(37.77),
        ArrivalTimeStd = TimeSpan.FromSeconds(8),
        InitialNumberOfBlocks = 34,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_23AM
    {
      get => new Settings()
      {
        // 23-AM
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(4),
        CraneMoveTimeStd = TimeSpan.FromSeconds(1),
        HoistMoveTimeMean = TimeSpan.FromSeconds(2),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.5),
        DueTimeMean = TimeSpan.FromSeconds(1731),
        DueTimeStd = TimeSpan.FromSeconds(300),
        ArrivalTimeMean = TimeSpan.FromSeconds(36.06),
        ArrivalTimeStd = TimeSpan.FromSeconds(8),
        InitialNumberOfBlocks = 34,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,
        ReadyFactorMax = 0.2,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_23BE
    {
      get => new Settings()
      {
        // 23-BE
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(0.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(4),
        HoistMoveTimeStd = TimeSpan.FromSeconds(1),
        DueTimeMean = TimeSpan.FromSeconds(1756.8),
        DueTimeStd = TimeSpan.FromSeconds(300),
        ArrivalTimeMean = TimeSpan.FromSeconds(39.21),
        ArrivalTimeStd = TimeSpan.FromSeconds(8),
        InitialNumberOfBlocks = 34,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }


    public static Settings settings_23BM
    {
      get => new Settings()
      {
        // 23-BM
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(0.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(4),
        HoistMoveTimeStd = TimeSpan.FromSeconds(1),
        DueTimeMean = TimeSpan.FromSeconds(1845.6),
        DueTimeStd = TimeSpan.FromSeconds(300),
        ArrivalTimeMean = TimeSpan.FromSeconds(38.46),
        ArrivalTimeStd = TimeSpan.FromSeconds(8),
        InitialNumberOfBlocks = 34,
        CheckInterval = TimeSpan.FromSeconds(.5),
        Seed = 42,
        DueTimeMin = TimeSpan.FromSeconds(60),
        ReadyFactorMin = 0.1,

        MinClearTime = TimeSpan.FromSeconds(0),
        MaxClearTime = TimeSpan.FromSeconds(1),
        HandoverTimeMean = TimeSpan.FromSeconds(4),
        HandoverTimeStd = TimeSpan.FromSeconds(1),

        //SimulationDuration = TimeSpan.FromSeconds(10),
        //SimulationDuration = TimeSpan.FromMinutes(1),
        SimulationDuration = TimeSpan.FromHours(1)
      };
    }
  }
}
