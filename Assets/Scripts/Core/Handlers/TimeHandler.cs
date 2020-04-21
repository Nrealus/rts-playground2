using UnityEngine;
using Core.Selection;
using VariousUtilsExtensions;
using Core.Units;
using Gamelogic.Extensions;

namespace Core.Handlers
{
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