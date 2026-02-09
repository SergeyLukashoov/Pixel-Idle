using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils {
  public class RandomProbInfo {
    public string m_value = "";
    public float  m_start = 0;
    public float  m_end   = 0;
  }

  public class RandomProb {
    private List<RandomProbInfo> m_values  = new List<RandomProbInfo>();
    private float                m_maximum = 0;

    public int Count => m_values.Count;
    
    public void AddValue(string value, float prob) {
      var randomProbInfo = new RandomProbInfo();

      randomProbInfo.m_value = value;
      randomProbInfo.m_start = m_maximum;
      randomProbInfo.m_end   = randomProbInfo.m_start + prob;

      m_maximum = randomProbInfo.m_end;
      m_values.Add(randomProbInfo);
    }


    public string GetRandomValue() {
      var random      = Random.Range(0f, 1f) * m_maximum;
      var randomValue = "";

      for (var i = 0; i < m_values.Count; ++i)
        if (random >= m_values[i].m_start && random < m_values[i].m_end)
          return m_values[i].m_value;

      return randomValue;
    }
  }
}