using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandControlTool
{
    public class DirectionOf9History
    {
        public enum Type
        {
            Zero = 0,
            AllSameDirection = 1,
            MostDirection = 2
        }
        public struct DirectionAndTime
        {
            public float time;
            public Vector2 direction;
        }

        public Queue<DirectionAndTime> history = new Queue<DirectionAndTime>();
        float fastHistoryTime = 0.11f;
        float zeroCutTime = 0.151f;
        float historyTime = 0.21f;

        public void GetDirectionOfNineFromHistory(out Vector2 direction, out Type type)
        {
            bool allZeroInCutTime = true;
            foreach (DirectionAndTime clip in history)
            {
                if (Time.fixedTime - clip.time <= zeroCutTime)
                {
                    if (clip.direction.magnitude != 0)
                    {
                        allZeroInCutTime = false;
                        break;
                    }
                }
            }
            if (allZeroInCutTime)
            {
                direction = new Vector2();
                type = Type.Zero;
                return;
            }

            //-----------------------------------------------------------
            bool allSameAndNotZeroInFastHistory = false;
            Vector2 directionByFastHistory = new Vector2();
            bool directionFound = false;
            foreach (DirectionAndTime clip in history)
            {
                if (Time.fixedTime - clip.time <= fastHistoryTime)
                {
                    if (clip.direction.magnitude != 0)
                    {
                        if (!directionFound)
                        {
                            allSameAndNotZeroInFastHistory = true;
                            directionByFastHistory = clip.direction;
                            directionFound = true;
                        }
                        else
                        {
                            if (clip.direction != directionByFastHistory)
                            {
                                allSameAndNotZeroInFastHistory = false;
                            }
                        }
                    }
                }
            }
            if (allSameAndNotZeroInFastHistory && directionByFastHistory.magnitude != 0f)
            {
                direction = directionByFastHistory;
                type = Type.AllSameDirection;
                return;
            }

            //-----------------------------------------------------------
            //把8个方向放在这里，然后数count。可以用 vector2 的 dictionary，但是我搜了搜，需要实现GetHashCode和Equals，就先不搞了。
            List<Vector2> possibleDirections = new List<Vector2>();
            possibleDirections.Add(new Vector2(1, 0).normalized);
            possibleDirections.Add(new Vector2(-1, 0).normalized);
            possibleDirections.Add(new Vector2(0, 1).normalized);
            possibleDirections.Add(new Vector2(0, -1).normalized);
            possibleDirections.Add(new Vector2(1, 1).normalized);
            possibleDirections.Add(new Vector2(1, -1).normalized);
            possibleDirections.Add(new Vector2(-1, 1).normalized);
            possibleDirections.Add(new Vector2(-1, -1).normalized);
            List<int> count = new List<int>();
            foreach (Vector2 dir in possibleDirections) count.Add(0);

            foreach (var clip in history)
            {
                if (Time.fixedTime - clip.time <= historyTime)
                {
                    for (int i = 0; i < possibleDirections.Count; i++)
                    {
                        if (possibleDirections[i] == clip.direction)
                        {
                            count[i]++;
                        }
                    }
                }
            }

            int maxCount = 0;
            Vector2 maxCountDirection = new Vector2();
            for (int i = 0; i < count.Count; i++)
            {
                if (count[i] > maxCount)
                {
                    maxCount = count[i];
                    maxCountDirection = possibleDirections[i];
                }
            }

            //可能出现两个方向的Count相同。不处理。根据代码选到谁就是谁啦。

            if (maxCount > 0)
            {
                direction = maxCountDirection;
                type = Type.MostDirection;
                return;
            }

            //前面的处理必定有一个结果，如果代码走到这里，说明有bug
            //因为前面已经检查过，在speedCutTime内，不都为0了。所以，这里检查historyTime,不应该出现全为0的情况。
            direction = new Vector2();
            type = Type.Zero;
            throw new System.Exception("Code here should not be runed. It should have results and return before.");
        }

        public void FixedUpdateManually(float time, Vector2 direction)
        {
            if (!Tool.nineDirections.Contains(direction))
            {
                throw new System.Exception("invalid direction");
            }

            DirectionAndTime newClip = new DirectionAndTime { time = time, direction = direction };
            history.Enqueue(newClip);
            if (history.Count > 0)
            {
                while ((Time.fixedTime - history.Peek().time) > historyTime)
                {
                    history.Dequeue();
                    if (history.Count == 0)
                    {
                        throw new System.Exception("history.Count should not be zero. There may be problems.");
                    }
                }
            }
            
        }
    }
}