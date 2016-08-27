namespace ScaleLibrary {
	using System;
	/// <summary>
	/// Result from GetWeight
	/// </summary>
	public struct WeightResult {
		/// <summary>
		/// The weight
		/// </summary>
		public double Weight {
			get;
			private set;
		}
		/// <summary>
		/// The Status of the scale
		/// </summary>
		public ScaleStatus Status {
			get;
			private set;
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="status"></param>
		public WeightResult(double weight, ScaleStatus status) : this() {
			Weight = weight;
			Status = status;
		}
	}
}
