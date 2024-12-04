using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LessonEra
{
    public class Counters : MonoBehaviour
    {
        #region Constants
        public const float NegativeValue = -5;
        #endregion

        #region Singlton
        public static Counters Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(this);
                Debug.Log("Destroied");
            }
        }
        #endregion

        #region Fields
        Dictionary<string, CountDown> countdowns = new Dictionary<string, CountDown>();
        #endregion

        #region CountDowns
        /// <summary>
        /// call start Counter after implenent new counter or editing old counter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Maximium"></param>
        /// <param name="AtFinish"></param>
        public void AddCountDown(string name, float Maximium, Action AtFinish)
        {
            if (countdowns.ContainsKey(name))
            {
                countdowns[name].MaximumValue = Maximium;
                countdowns[name].currentValue = Maximium;
                countdowns[name].lastValue = (int)Maximium;
                countdowns[name].AtFinish = AtFinish;
                return;
            }

            countdowns.Add(name, new CountDown { MaximumValue = Maximium, currentValue = Maximium,
                lastValue = (int)Maximium, IsTicking = false, AtFinish = AtFinish });
        }

        /// <summary>
        /// call start Counter after implenent new counter or editing old counter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Maximium"></param>
        /// <param name="AtFinish"></param>
        public void AddCountDown(string name, float Maximium, Dictionary<byte, 
            KeyValuePair<bool, Action>> OnCertainValue, Action AtFinish)
        {
            Debug.Log($"timer is added with name : {name}");
            if (countdowns.ContainsKey(name))
            {
                countdowns[name].MaximumValue = Maximium;
                countdowns[name].currentValue = Maximium;
                countdowns[name].lastValue = (int)Maximium;
                countdowns[name].OnCertainValue = OnCertainValue;
                countdowns[name].AtFinish = AtFinish;
                return;
            }

            countdowns.Add(name, new CountDown
            {
                MaximumValue = Maximium,
                currentValue = Maximium,
                OnCertainValue = OnCertainValue,
                IsTicking = false,
                lastValue = (int)Maximium,
                AtFinish = AtFinish
            });
        }

        /// <summary>
        /// call start Counter after implenent new counter or editing old counter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Maximium"></param>
        /// <param name="AtFinish"></param>
        public void AddCountDown(string name, float Maximium, Action OnTick, Action AtFinish)
        {
            if (countdowns.ContainsKey(name))
            {
                countdowns[name].MaximumValue = Maximium;
                countdowns[name].currentValue = Maximium;
                countdowns[name].lastValue = (int)Maximium;
                countdowns[name].OnEachSecond = OnTick;
                countdowns[name].AtFinish = AtFinish;
                return;
            }

            countdowns.Add(name, new CountDown
            {
                MaximumValue = Maximium,
                currentValue = Maximium,
                lastValue = (int)Maximium,
                OnEachSecond = OnTick,
                IsTicking = false,
                AtFinish = AtFinish
            });
        }

        /// <summary>
        /// call start Counter after implenent new counter or editing old counter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Maximium"></param>
        /// <param name="AtFinish"></param>
        public void AddCountDown(string name, float Maximium, Action OnTick, Dictionary<byte,
            KeyValuePair<bool, Action>> OnCertainValue, Action AtFinish)
        {
            Debug.Log($"timer is added with name : {name}");

            if (countdowns.ContainsKey(name))
            {
                countdowns[name].MaximumValue = Maximium;
                countdowns[name].currentValue = Maximium;
                countdowns[name].lastValue = (int)Maximium;
                countdowns[name].OnCertainValue = OnCertainValue;
                countdowns[name].OnEachSecond = OnTick;
                countdowns[name].AtFinish = AtFinish;
                return;
            }

            countdowns.Add(name, new CountDown
            {
                MaximumValue = Maximium,
                currentValue = Maximium,
                OnCertainValue = OnCertainValue,
                lastValue = (int)Maximium,
                OnEachSecond = OnTick,
                IsTicking = false,
                AtFinish = AtFinish
            });
        }

        public void AddCountDownOnCertainValueAction(string name, byte value, Action action)
        {
            if (!countdowns.ContainsKey(name))
            {
                Debug.LogError($"no countdown with this name {name}");
                return;
            }

            if (countdowns[name].OnCertainValue.IsNull())
            {
                countdowns[name].OnCertainValue = new();
            }

            if (countdowns[name].currentValue < value)
            {
                Debug.LogError($"{value} exceeded in current counter: so can't be added");
                return;
            }

            if (countdowns[name].OnCertainValue.ContainsKey(value))
            {
                Debug.LogWarning($"{value} already exist in current counter");

                countdowns[name].OnCertainValue[value] = new(false, action);
            }
            else
            {
                countdowns[name].OnCertainValue.Add(value, new(false, action));
            }
        }

        public void AddCountDownOnTickAction(string name, Action action)
        {
            if (!countdowns.ContainsKey(name))
            {
                Debug.LogError($"no countdown with this name {name}");
                return;
            }

            countdowns[name].OnEachSecond = action;
        }

        public void ModifyCountdownValue(string name, float value)
        {
            if (countdowns.ContainsKey(name))
            {
                countdowns[name].currentValue = value;
                return;
            }

            Debug.Log($"no count down with that name {name}");
        }

        public bool HasCountDown(string name)
        {
            if (countdowns.ContainsKey(name))
            {
                return countdowns[name].currentValue > 0;
            }
            return false;
        }

        public bool IsCounterDownActive(string name)
        {
            if (countdowns.ContainsKey(name))
            {
                return countdowns[name].IsTicking;
            }
            return false;
        }

        public float GetCountDownCurrentValue(string name)
        {
            if (countdowns.ContainsKey(name))
            {
                return countdowns[name].currentValue;
            }
            return NegativeValue;
        }

        public List<string> GetCounterKeys()
        {
            return countdowns.Keys.ToList();
        }

        Action WaitUntillEndOfUpdateExcution_BugFixed;
        public void RemoveCountDown_WaitUntillEndOfUpdateExcusion(string name)
        {
            Debug.Log($"Add Remove Timer {name}");
            WaitUntillEndOfUpdateExcution_BugFixed += () =>
            {
                Debug.Log($"Removed Timer {name}");
                if (countdowns.ContainsKey(name))
                    countdowns.Remove(name);
            };
        }

        void RemoveCountDownImmediately(string name)
        {
            Debug.Log($"Remove Timer {name}");
            if (countdowns.ContainsKey(name))
                countdowns.Remove(name);
        }


        public void StartCountDown(string name)
        {
            if(countdowns.ContainsKey(name))
            {
                Debug.Log($"timerStarted {name}" + countdowns[name].currentValue);                
                countdowns[name].IsTicking = true; 
            }
            else
            {
                Debug.Log($"error while starting countDown called {name}");
            }
        }

        public void StopCountDown(string name)
        {
            if (countdowns.ContainsKey(name))
            {
                countdowns[name].IsTicking = false;
            }
            else
            {
                Debug.Log($"error while stop countDown called {name}");
            }
        }
        #endregion

        #region Handle All Counters
        private void Update()
        {

            for(int i = 0; i < countdowns.Count; i++)
            {
                List<string> keys = countdowns.Keys.ToList();
                string key = keys[i];
                if (countdowns[key].IsTicking)
                {
                    countdowns[key].currentValue -= Time.deltaTime;

                    byte value = (byte)countdowns[key].currentValue;
                    
                    if (countdowns[key].HasActionOnCertainValue(value))
                    {                       
                        // You should call the action before resetting the entry and marking it as complete.
                        countdowns[key].OnCertainValue[value].Value?.Invoke();
                        countdowns[key].OnCertainValue[value] = new KeyValuePair<bool, Action>(true, null);
                    }

                    if (!countdowns[key].OnEachSecond.IsNull())
                    {
                        if (countdowns[key].lastValue != (int)countdowns[key].currentValue)
                        {
                            countdowns[key].lastValue = (int)countdowns[key].currentValue;
                            countdowns[key].OnEachSecond?.Invoke();
                        }
                    }

                    if (countdowns[key].currentValue <= 0)
                    {
                        Action finish = countdowns[key].AtFinish;
                        RemoveCountDownImmediately(key);
                        finish?.Invoke();
                    }
                }

                WaitUntillEndOfUpdateExcution_BugFixed?.Invoke();
                WaitUntillEndOfUpdateExcution_BugFixed = null;
            }
        }
        #endregion

        #region CountDown Class
        [Serializable]
        class CountDown
        {
            public float MaximumValue;
            public bool IsTicking;
            public float currentValue;
            public int lastValue;

            public Action OnEachSecond;
            public Dictionary<byte, KeyValuePair<bool, Action>> OnCertainValue;
            public Action AtFinish;

            public bool HasActionOnCertainValue(byte value)
            {
                if (!OnCertainValue.IsNull())
                {
                    if (OnCertainValue.Count > 0)
                    {
                        if (OnCertainValue.ContainsKey(value))
                        {
                            if (!OnCertainValue[value].Key)
                            {
                                return true; 
                            }
                        }
                    }
                }

                return false;
            }
        }
        #endregion
    }
}
