namespace ScaleLibrary {
	using System;
	/// <summary>
	/// The scale status flags
	/// </summary>
	public enum ScaleStatus :int {
		UNKNOWN = 0,
		FAULT = 1,
		STABLE_0 = 2,
		IN_MOTION = 4,
		STABLE = 4,
		UNDER_ZERO = 5,
		OVER_WEIGHT = 6,
		REQUIRES_CALIBRATION = 7,
		REQUIRES_TARE = 8
	}
}
