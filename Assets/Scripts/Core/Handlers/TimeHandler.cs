using UnityEngine;
using Core.Selection;
using VariousUtilsExtensions;
using Core.Units;
using Gamelogic.Extensions;

namespace Core.Handlers
{
    /// <summary>
    /// ---- General Description, by nrealus, last update : 23-04-2020 ----
    ///
    /// Singleton used to oversee time related things on the scale of the game, effectively acting as a global clock.
    /// For now, it runs a day:hour:minute format clock at a rate 200x faster than real time.
    /// In the future, other handlers updates (which update other stuff themselves) should depend on the updates of this clock.
    /// </summary>    
    public class TimeHandler : MonoBehaviour
    {

        private static TimeHandler _instance;
        private static TimeHandler MyInstance
        {
            get
            {
                if(_instance == null)
                    _instance = FindObjectOfType<TimeHandler>(); 
                return _instance;
            }
        }

        public static string TimeStructToString(TimeStruct timeStruct)
        {
            return string.Format("Day {0}, {1:D2}:{2:D2}", timeStruct.day, timeStruct.hour, timeStruct.minute);
        }

        public static string CurrentTimeToString()
        {
            return TimeStructToString(MyInstance.timeClock.timeValue);
        }

        public static TimeStruct CurrentTime()
        {
            return MyInstance.timeClock.timeValue;
        }

        public static bool HasTimeJustPassed(TimeStruct time)
        {
            return MyInstance.timeClock.HasClockJustPassed(time);
        }

        public static bool HasTimeAlreadyPassed(TimeStruct time)
        {
            return MyInstance.timeClock.HasClockAlreadyPassed(time);
        }

        public TimeClock timeClock = new TimeClock(new TimeStruct(9,14,30));

        private void Start()
        {
            timeClock.Resume();
        }

        private void Update()
        {
            timeClock.Update(200f, Time.deltaTime);
        }

    }
}