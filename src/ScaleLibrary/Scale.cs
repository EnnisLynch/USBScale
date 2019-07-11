namespace ScaleLibrary {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using HidLibrary;
	 //This is a test
	/// <summary>
	/// Will read data from the scale
	/// </summary>
	/// <remarks>
	/// Where does it all come from?
	/// http://www.usb.org/developers/hidpage/pos1_02.pdf
	/// 
	/// And, unlike other examples on the Internet this one
	/// knows how to tare and scale
	/// </remarks>
	public class Scale : IDisposable {
		private IHidDevice mScale = null;
		/// <summary>
		/// Is this disposed
		/// </summary>
		public bool IsDisposed {
			get;
			private set;
		}
		/// <summary>
		/// Will Tare the scale
		/// </summary>
		/// <remarks>
		///Page 56 http://www.usb.org/developers/hidpage/pos1_02.pdf
		/// Byte 0 must be 2 to set report id
		/// Byte 1 first bit = enforce zero return ie 0x01
		/// Byte 1 second bit = tare ie 0x02
		/// </remarks>
		public void Tare() {
			CheckStatusAndThrow();
			mScale.Write(new byte[] { 0x02, 0x02 });
		}
		/// <summary>
		/// Will process the byte array
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public WeightResult HelpGetWeight(byte[] data) {
			double weight = 0;
			ScaleStatus status = ScaleStatus.UNKNOWN;
			// Byte 0 == Report ID?
			// Byte 1 == Scale Status (1 == Fault, 2 == Stable @ 0, 3 == In Motion, 4 == Stable, 5 == Under 0, 6 == Over Weight, 7 == Requires Calibration, 8 == Requires Re-Zeroing)
			// Byte 2 == Weight Unit
			// Byte 3 == Data Scaling (decimal placement) 10^-127 to 127
			// Byte 4 == Weight LSB
			// Byte 5 == Weight MSB
			// Byte 3
			//Don't use convert as it will thrown an exception
			sbyte signedByte = (sbyte)data[3];
			double scale = Math.Pow(10, signedByte);

			weight = scale * (data[4] + (256 * data[5]));
			switch (data[2]) {
				case 3:  // Kilos
					weight *= 2.2;
					break;
				case 11: // Ounces
					weight *= 0.625;
					break;
				case 12: // Pounds
					// already in pounds, do nothing
					break;
			}
			status = (ScaleStatus)(data[1]);
			if (data[1] > 8) {
				status = ScaleStatus.UNKNOWN;
			}
			return new WeightResult(weight, status);
		}
		/// <summary>
		/// Will return the weight
		/// </summary>
		/// <remarks>
		/// A little help from http://stackoverflow.com/questions/11961412/read-weight-from-a-fairbanks-scb-9000-usb-scale
		/// for the full conversion
		/// </remarks>
		/// <param name="weight"></param>
		/// <param name="isStable"></param>
		/// <returns></returns>
		public WeightResult GetWeight() {
			CheckStatusAndThrow();
			WeightResult result = new WeightResult();
			if (mScale.IsConnected) {
				HidDeviceData inData = mScale.Read(10);
				if (inData.Status != HidDeviceData.ReadStatus.Success) {
					throw new Exception("Data Read Failure");
				}
				result = HelpGetWeight(inData.Data);
			}//end if is connected
			return result;
			
		}
		/// <summary>
		/// There are a couple of error conditions to
		/// throw up on, keep them in one place
		/// </summary>
		private void CheckStatusAndThrow() {
			if (IsDisposed) {
				throw new ObjectDisposedException("Scale");
			}
			if (mScale == null) {
				throw new Exception("Not Connected");
			}
		}
		/// <summary>
		/// Will initialize the scale that matches the provided vendor id
		/// </summary>
		public void Connect() {
			if (IsDisposed) {
				throw new ObjectDisposedException("Scale");
			}
			foreach (var device in HidDevices.Enumerate(0x0922, 0x8004)) {
				mScale = device;
				mScale.Removed += mScale_Removed;
				mScale.OpenDevice();
				int tries = 0;
				while (!mScale.IsConnected) {
					System.Threading.Thread.Sleep(10);
					tries++;

					if (tries > 50) {
						throw new Exception("Unable to Initialize Scale");
					}
				}
				Tare();
				break;
			}//end start scale
		}
		/// <summary>
		/// Clears this out when the removed 
		/// event happens
		/// </summary>
		private void mScale_Removed() {
			if (mScale != null) {
				mScale.Dispose();
				mScale = null;
			}
		}
		/// <summary>
		/// Will disconnect the scale
		/// </summary>
		public void Disconnect() {
			if (mScale != null) {
				mScale.CloseDevice();
				mScale.Dispose();
				mScale = null;
			}
		}
		/// <summary>
		/// Will dispose
		/// </summary>
		public void Dispose() {
			IsDisposed = true;
			Disconnect();
		}
		/// <summary>
		/// Will dispose
		/// </summary>
		~Scale() {
			Dispose();
		}
		/// <summary>
		/// Create this object
		/// </summary>
		public Scale() {
			IsDisposed = false;
		}
	}
}
