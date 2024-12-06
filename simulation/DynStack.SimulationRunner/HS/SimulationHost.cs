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
          case "EmptySettings_213A":
            settings = EmptySettings_213A;
            break;
          case "FullSettings_213A":
            settings = FullSettings_213A;
            break;
          case "settings_213B":
            settings = settings_213B;
            break;
          case "settings_213D":
            settings = settings_213D;
            break;
          case "EmptySettings_216A":
            settings = EmptySettings_216A;
            break;
          case "FullSettings_216A":
            settings = FullSettings_216A;
            break;
          case "settings_216B":
            settings = settings_216B;
            break;
          case "settings_216D":
            settings = settings_216D;
            break;
          case "EmptySettings_219A":
            settings = EmptySettings_219A;
            break;
          case "FullSettings_219A":
            settings = FullSettings_219A;
            break;
          case "settings_219A":
            settings = settings_219A;
            break;
          case "settings_219B":
            settings = settings_219B;
            break;
          case "settings_219C":
            settings = settings_219C;
            break;
          case "settings_219D":
            settings = settings_219D;
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

    public static Settings EmptySettings_213A
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
        InitialNumberOfBlocks = 0,
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

    public static Settings FullSettings_213A
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
        InitialNumberOfBlocks = 24,
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
    public static Settings settings_213B
    {
      get => new Settings()
      {
        // settings_213B
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 3,

        CraneMoveTimeMean = TimeSpan.FromSeconds(1.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(0.5),

        HoistMoveTimeMean = TimeSpan.FromSeconds(.25),
        HoistMoveTimeStd = TimeSpan.FromSeconds(1),

        DueTimeMean = TimeSpan.FromSeconds(170),
        DueTimeStd = TimeSpan.FromSeconds(300),

        ArrivalTimeMean = TimeSpan.FromSeconds(10.62),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.12),

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


    public static Settings settings_213D
    {
      get => new Settings()
      {
        // settings_213D
        ProductionMaxHeight = 4,
        BufferMaxHeight = 8,
        BufferCount = 3,

        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.3),

        HoistMoveTimeMean = TimeSpan.FromSeconds(.5),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.05),

        DueTimeMean = TimeSpan.FromSeconds(468),
        DueTimeStd = TimeSpan.FromSeconds(24),
        ArrivalTimeMean = TimeSpan.FromSeconds(16.25),
        ArrivalTimeStd = TimeSpan.FromSeconds(3.25),
        InitialNumberOfBlocks = 14,
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


    public static Settings EmptySettings_216A
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
        InitialNumberOfBlocks = 0,
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


    public static Settings FullSettings_216A
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
        InitialNumberOfBlocks = 36,
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


    public static Settings settings_216B
    {
      get => new Settings()
      {
        // settings_216B
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.4),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.5),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.1),
        DueTimeMean = TimeSpan.FromSeconds(480),
        DueTimeStd = TimeSpan.FromSeconds(96),
        ArrivalTimeMean = TimeSpan.FromSeconds(12.5),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.5),
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


    public static Settings settings_216D
    {
      get => new Settings()
      {

        // 21-6D
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 6,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.4),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.5),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.1),
        DueTimeMean = TimeSpan.FromSeconds(267),
        DueTimeStd = TimeSpan.FromSeconds(52.8),
        ArrivalTimeMean = TimeSpan.FromSeconds(18.13),
        ArrivalTimeStd = TimeSpan.FromSeconds(3.63),
        InitialNumberOfBlocks = 22,
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


    public static Settings EmptySettings_219A
    {
      get => new Settings()
      {

        // settings_219A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.75),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.15),
        DueTimeMean = TimeSpan.FromSeconds(946),
        DueTimeStd = TimeSpan.FromSeconds(189.2),
        ArrivalTimeMean = TimeSpan.FromSeconds(16.43),
        ArrivalTimeStd = TimeSpan.FromSeconds(3.29),
        InitialNumberOfBlocks = 0,
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


    public static Settings FullSettings_219A
    {
      get => new Settings()
      {

        // settings_219A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.75),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.15),
        DueTimeMean = TimeSpan.FromSeconds(946),
        DueTimeStd = TimeSpan.FromSeconds(189.2),
        ArrivalTimeMean = TimeSpan.FromSeconds(16.43),
        ArrivalTimeStd = TimeSpan.FromSeconds(3.29),
        InitialNumberOfBlocks = 54,
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


    public static Settings settings_219A
    {
      get => new Settings()
      {

        // settings_219A
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.75),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.15),
        DueTimeMean = TimeSpan.FromSeconds(946),
        DueTimeStd = TimeSpan.FromSeconds(189.2),
        ArrivalTimeMean = TimeSpan.FromSeconds(16.43),
        ArrivalTimeStd = TimeSpan.FromSeconds(3.29),
        InitialNumberOfBlocks = 32,
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


    public static Settings settings_219B
    {
      get => new Settings()
      {

        // settings_219B
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.75),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.15),
        DueTimeMean = TimeSpan.FromSeconds(725),
        DueTimeStd = TimeSpan.FromSeconds(145),
        ArrivalTimeMean = TimeSpan.FromSeconds(14.37),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.87),
        InitialNumberOfBlocks = 27,
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


    public static Settings settings_219C
    {
      get => new Settings()
      {

        // settings_219C
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(2.5),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.5),
        HoistMoveTimeMean = TimeSpan.FromSeconds(.75),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.15),
        DueTimeMean = TimeSpan.FromSeconds(552),
        DueTimeStd = TimeSpan.FromSeconds(110.4),
        ArrivalTimeMean = TimeSpan.FromSeconds(12.78),
        ArrivalTimeStd = TimeSpan.FromSeconds(2.56),
        InitialNumberOfBlocks = 22,
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


    public static Settings settings_219D
    {
      get => new Settings()
      {

        // settings_219D
        ProductionMaxHeight = 4,
        BufferMaxHeight = 6,
        BufferCount = 9,
        CraneMoveTimeMean = TimeSpan.FromSeconds(3),
        CraneMoveTimeStd = TimeSpan.FromSeconds(.6),
        HoistMoveTimeMean = TimeSpan.FromSeconds(1),
        HoistMoveTimeStd = TimeSpan.FromSeconds(.2),
        DueTimeMean = TimeSpan.FromSeconds(1728),
        DueTimeStd = TimeSpan.FromSeconds(345.6),
        ArrivalTimeMean = TimeSpan.FromSeconds(20),
        ArrivalTimeStd = TimeSpan.FromSeconds(4),
        InitialNumberOfBlocks = 32,
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


  }
}
