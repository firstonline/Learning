using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
   public int AverageFps { get; private set; }
   public int HighestFps { get; private set; }
   public int LowestFps { get; private set; }

   public int m_frameRange = 60;
   private int[] m_fpsBuffer;
   private int m_fpsBufferIndex;

   private void Update()
   {
      if (m_fpsBuffer == null || m_fpsBuffer.Length != m_frameRange)
      {
         InitializeBuffer();
      }

      UpdateBuffer();
      CalculateAverage();
   }

   private void InitializeBuffer()
   {
      if (m_frameRange <= 0)
      {
         m_frameRange = 1;
      }

      m_fpsBuffer = new int[m_frameRange];
      m_fpsBufferIndex = 0;
   }


   private void UpdateBuffer()
   {
      m_fpsBuffer[m_fpsBufferIndex++] = (int) (1f / Time.unscaledDeltaTime);
      if (m_fpsBufferIndex == m_fpsBuffer.Length)
      {
         m_fpsBufferIndex = 0;
      }
   }

   private void CalculateAverage()
   {
      int sum = 0;
      int highest = 0;
      int lowest = int.MaxValue;

      for (int i = 0; i < m_frameRange; i++)
      {
         int fps = m_fpsBuffer[i];
         if (fps > highest)
         {
            highest = fps;
         }

         if (fps < lowest)
         {
            lowest = fps;
         }

         sum += fps;
      }

      AverageFps = sum / m_frameRange;
      HighestFps = highest;
      LowestFps = lowest;
   }
}
