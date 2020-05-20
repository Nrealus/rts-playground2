using Gamelogic.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VariousUtilsExtensions
{
    /****** Author : nrealus ****** Last documentation update : 20-05-2020 ******/

    ///<summary>
    /// A minimal structure representing and storing time in a Day:Hour:Minute format.
    ///</summary>
    public struct TimeStruct
    {
        public static TimeStruct OneMinute = new TimeStruct(0,0,1);

        public static TimeStruct Zero = new TimeStruct(0,0,0);

        public int day { get; private set; }
        public int hour { get; private set; }
        public int minute { get; private set; }

        public bool isInitialized { get; private set; }

        public TimeStruct(int day, int hour, int minute)
        {
            isInitialized = true;

            this.day = day;
            this.hour = hour;
            this.minute = minute;
            Adjust(); // VERY IMPORTANT
        }

        public override bool Equals(object obj)
        {
            
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            
            if (obj == null || GetType() != obj.GetType())
            {
                var exc = new System.SystemException("Warning, don't compare a struct to 'null' !!!");
                UnityEngine.Debug.LogException(exc);
                throw exc;
                //return !isInitialized;
            }
            else
            {
                TimeStruct castedObj = (TimeStruct)obj;
                
                if (!isInitialized || !castedObj.isInitialized)
                {
                    var exc = new System.SystemException("One of the TimeStructs is not initialized");
                    UnityEngine.Debug.LogException(exc);
                    throw exc;
                }

                return day == castedObj.day && hour == castedObj.hour && minute == castedObj.minute;
            }

        }
        
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            //throw new System.NotImplementedException();
            return base.GetHashCode();
        }

        public static TimeStruct operator +(TimeStruct time1, TimeStruct time2)
        {
            if(!time1.isInitialized || !time2.isInitialized)
            {
                var exc = new System.SystemException("One of the TimeStructs is not initialized");
                UnityEngine.Debug.LogException(exc);
                throw exc;
            }

            TimeStruct res = new TimeStruct(
                time1.day + time2.day,
                time1.hour + time2.hour,
                time1.minute + time2.minute);
            return res;
        }

        public static TimeStruct operator -(TimeStruct time1, TimeStruct time2)
        {
            if(!time1.isInitialized || !time2.isInitialized)
            {
                var exc = new System.SystemException("One of the TimeStructs is not initialized");
                UnityEngine.Debug.LogException(exc);
                throw exc;
            }

            return time1 + (-time2);
        }

        public static TimeStruct operator -(TimeStruct time1)
        {
            return new TimeStruct(-time1.day, -time1.hour, -time1.minute);
        }

        public static bool operator ==(TimeStruct time1, TimeStruct time2)
        {
            return time1.Equals(time2);
        }

        public static bool operator !=(TimeStruct time1, TimeStruct time2)
        {
            return !time1.Equals(time2);
        }

        public static bool operator >(TimeStruct time1, TimeStruct time2)
        {
            if(!time1.isInitialized || !time2.isInitialized)
            {
                var exc = new System.SystemException("One of the TimeStructs is not initialized");
                UnityEngine.Debug.LogException(exc);
                throw exc;
            }

            return time1.day > time2.day
                || (time1.day == time2.day && time1.hour > time2.hour)
                || (time1.hour == time2.hour && time1.minute > time2.minute);
        }

        public static bool operator <(TimeStruct time1, TimeStruct time2)
        {
            return time2 > time1;
        }

        public static bool operator >=(TimeStruct time1, TimeStruct time2)
        {
            return time1 == time2 || time1 > time2;
        }

        public static bool operator <=(TimeStruct time1, TimeStruct time2)
        {
            return time1 == time2 || time1 < time2;
        }

        public static int Compare(TimeStruct time1, TimeStruct time2)
        {
            if (time1==time2)
                return 0;
            else if (time1>time2)
                return 1;
            else
                return -1;
                
        }

        private void Adjust()
        {
            int newMinute = FloorDivisionRemainder(minute, 60);
            int newHour = FloorDivisionRemainder(hour, 24) + FloorDivisionQuotient(minute, 60);
            int newDay = day + FloorDivisionQuotient(hour, 24);

            minute = newMinute;
            hour = newHour;
            day = newDay;
        }

        private int FloorDivisionQuotient(int a, int b)
        {
            return (a/b - System.Convert.ToInt32(((a < 0) ^ (b < 0)) && (a % b != 0)));
        }
        private int FloorDivisionRemainder(int a, int b)
        {
            return a - b * FloorDivisionQuotient(a,b);
        }

    }

    ///<summary>
    /// A clock class for TimeStruct type time, which can be updated at any rate.!--
    /// It is also able to tell if a certain time (TimeStruct : day-hour-minute) has just passed, even if it was paused and then manually set to another time.
    ///</summary>
    public class TimeClock
    {
        private Clock minuteTimer;

        public TimeStruct timeValue { get { return timeObserver.Value; } }
        private ObservedValue<TimeStruct> timeObserver;
        private TimeStruct previousTimeValue;

        public bool HasClockJustPassed(TimeStruct time)
        {
            return timeValue >= time && previousTimeValue < time;
        }

        public bool HasClockAlreadyPassed(TimeStruct time)
        {
            return timeValue >= time;
        }

        public void Update(float realSecondsRatio, float deltaTime)
        {
            previousTimeValue = timeObserver.Value;
            minuteTimer.Update(realSecondsRatio * deltaTime);         
        }

        public void Resume()
        {
            minuteTimer.Unpause();
        }

        public void Pause()
        {
            minuteTimer.Pause();
        }

        public TimeClock()
        {
            BaseConstructor(TimeStruct.Zero);
        }

        public TimeClock(TimeStruct initialTime)
        {
            BaseConstructor(initialTime);
        }

        private void BaseConstructor(TimeStruct initialTime)
        {
            minuteTimer = new Clock();
            timeObserver = new ObservedValue<TimeStruct>(initialTime);

            minuteTimer.Reset(60f);
            minuteTimer.OnClockExpired += () => {
                timeObserver.Value += TimeStruct.OneMinute;
                minuteTimer.Reset(60f);
            };
        }

    }
}
