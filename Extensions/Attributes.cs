using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AppoMobi.Specials.Extensions;

public static class AttributesExtensions

{
	public static T GetAttributeFromItem<T>(this object instance, string propertyName) where T : Attribute
	{
		var attrType = typeof(T);
		var property = instance.GetType().GetProperty(propertyName);
		if (property != null)
			return (T)property.GetCustomAttributes(attrType, false).First();
		return null;
	}


	public static T GetAttribute<T>(this PropertyInfo instance) where T : Attribute
	{
		var attrType = typeof(T);
		var hasList = instance.GetCustomAttributes(attrType, false);
		if (hasList.Length > 0) return (T)hasList.First();
		return null;
	}

	/// <summary>
	///     ex: var attr = property.GetAttributeProperty<Export>("Name");
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="instance"></param>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	public static dynamic GetAttributeProperty<T>(this PropertyInfo instance, string propertyName) where T : Attribute
	{
		var attrType = typeof(T);
		var hasList = instance.GetCustomAttributes(attrType, false);
		if (hasList.Length > 0)
		{
			var myAttribute = (T)hasList.First();
			return Reflection.TryGetPropertyValueFor(myAttribute, propertyName);
		}

		return null;
	}


	//public static Int32 GetMaxLength<T>(Expression<Func<T, string>> propertyExpression)

	//{
	//    return GetPropertyAttributeValue<T, string, MaxLengthAttribute, Int32>(propertyExpression,
	//        attr => attr.Length);
	//}

	//Optional Extension method
	//public static Int32 GetMaxLength<T>(this T instance, Expression<Func<T, string>> propertyExpression)
	//{
	//    return GetMaxLength<T>(propertyExpression);
	//}


	//Required generic method to get any property attribute from any class
	public static TValue GetPropertyAttributeValue<T, TOut, TAttribute, TValue>(
		Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector)
		where TAttribute : Attribute
	{
		var expression = (MemberExpression)propertyExpression.Body;
		var propertyInfo = (PropertyInfo)expression.Member;
		var attr = propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

		if (attr == null)
			throw new MissingMemberException(typeof(T).Name + "." + propertyInfo.Name, typeof(TAttribute).Name);

		return valueSelector(attr);
	}


	public static object GetAttribute(this Type classType, Type typeAttribute)
	{
		return classType.GetCustomAttributes(typeAttribute, true).FirstOrDefault();
	}

	public static dynamic GetAttribute<T>(this Type classType)
	{
		var dnAttribute = classType.GetCustomAttributes(typeof(T), true).FirstOrDefault();

		return dnAttribute;
	}


	public static bool HasAttributeDefined(this PropertyInfo property, Type attr)
	{
		return Attribute.IsDefined(property, attr);
	}
}