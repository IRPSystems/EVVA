

//using System.Globalization;
//using System.Windows.Controls;
//using System.Windows;
//using Entities.Models;
//using DeviceCommunicators.Models;

//namespace Evva.ValidationRuls
//{
//    public class SetParamValidationRule : ValidationRule
//	{
//		public ParameterWrapper SetParam { get; set; }

//		public override ValidationResult Validate(object value, CultureInfo cultureInfo)		
//		{
//			if(!(SetParam.SetParamNode is MCU_ParamData mcuParam))
//				return ValidationResult.ValidResult;

//			if(mcuParam.Range == null)
//				return ValidationResult.ValidResult;


//			double d = double.NaN;
//			if (value is string str)
//			{				
//				bool res = double.TryParse(str, out d);
//				if(!res) 
//					return new ValidationResult(false, "The value should be numeric");
				
//			}
//			else if(value is double dbl)
//				d = dbl;

//			if(d < mcuParam.Range[0] || d > mcuParam.Range[1])
//				return new ValidationResult(false, "The value is out of range");


//			return ValidationResult.ValidResult;
//		}
//	}

//	public class ParameterWrapper : DependencyObject
//	{
//		public static readonly DependencyProperty SetParamNodeProperty =
//			 DependencyProperty.Register("SetParamNode", typeof(DeviceParameterData),
//			 typeof(ParameterWrapper));

//		public DeviceParameterData SetParamNode
//		{
//			get { return (DeviceParameterData)GetValue(SetParamNodeProperty); }
//			set { SetValue(SetParamNodeProperty, value); }
//		}
//	}

	
//}
