using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace PastaGameLibrary
{
	public interface IPInterpolation<T>
	{
		T GetInterpolation(T from, T to, float ratio);
	}

	public class PLerpInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * ratio;
		}
	}
	public class PSquareInterpolation : IPInterpolation<float>
	{
		int m_factor = 2;

		public PSquareInterpolation(int factor)
		{
			m_factor = factor;
		}

		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * (float)Math.Pow(ratio, m_factor);
		}
	}
	public class PReverseSquareInterpolation : IPInterpolation<float>
	{
		int m_factor = 3;

		public PReverseSquareInterpolation(int factor)
		{
			m_factor = factor;
		}

		public float GetInterpolation(float from, float to, float ratio)
		{
			ratio = (float)Math.Pow(ratio - 1, m_factor) * -1 + 1;
			return from + (to - from) * ratio;
		}
	}
	public class PSquareRootInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * (float)Math.Sqrt(ratio);
		}
	}
	public class PSineHalfInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * (float)(Math.Sin(ratio * Math.PI * 0.5f));
		}
	}
	public class PSineInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * (float)(Math.Sin(ratio * Math.PI * 2.0));
		}
	}
	public class PSinePositiveInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * (float)(Math.Sin(ratio * Math.PI));
		}
	}
	public class PSmoothstepInterpolation : IPInterpolation<float>
	{
		public float GetInterpolation(float from, float to, float ratio)
		{
			return from + (to - from) * ratio * ratio * (3 - 2 * ratio);
		}
	}
}
