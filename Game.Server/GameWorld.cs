using Arch.Core;
using Schedulers;
using System;

namespace Game.Server
{
    public class GameWorld
    {
        public Action? OnInitialize;
        public Action<float>? OnUpdate;
        public Action? OnShutdown;
        public World World { get; set; }
        public GameWorld() 
        {
            World = World.Create();

            //var jobScheduler = new JobScheduler(new JobScheduler.Config
            //{
            //    // Names the process "MyProgram0", "MyProgram1", etc.
            //    ThreadPrefixName = "Game.Server",

            //    // Automatically chooses threads based on your processor count
            //    ThreadCount = 0,

            //    // The amount of jobs that can exist in the queue at once without the scheduler spontaneously allocating and generating garbage.
            //    // Past this number, the scheduler is no longer Zero-Alloc!
            //    // Higher numbers slightly decrease performance and increase memory consumption, so keep this on the lowest possible end for your application.
            //    MaxExpectedConcurrentJobs = 64,

            //    // Enables or disables strict allocation mode: if more jobs are scheduled at once than MaxExpectedConcurrentJobs, it throws an error.
            //    // Not recommended for production code, but good for debugging allocation issues.
            //    StrictAllocationMode = false,
            //});

            //World.SharedJobScheduler = jobScheduler;
        }
    }
}
