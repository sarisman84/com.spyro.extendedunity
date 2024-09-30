#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Spyro.EditorExtensions
{
    public static class SerializedPropertyUtilities
    {
        private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

        /// <summary>
        /// Iterates through each of the child properties found within a SerializedObject.
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public static IEnumerable<SerializedProperty> FindVisibleChildProperties(this SerializedObject parentObject)
        {
            var iterator = parentObject.GetIterator();
            iterator.NextVisible(true);
            do
            {
                yield return iterator.Copy();
            } while (iterator.NextVisible(false));


        }
        /// <summary>
        /// Iterates through each of the child properties found within a SerializedProperty.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static IEnumerable<SerializedProperty> FindVisibleChildProperties(this SerializedProperty parent)
        {
            var iterator = parent.Copy();
            iterator.NextVisible(true);

            do
            {
                yield return iterator.Copy();
            } while (iterator.NextVisible(false));

        }

        //Taken from: https://gist.github.com/starikcetin/583a3b86c22efae35b5a86e9ae23f2f0

        /// <summary>
        /// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
        /// </summary>
        public static TAttribute[] GetAttributes<TAttribute>(this SerializedProperty serializedProperty, bool inherit)
            where TAttribute : Attribute
        {
            if (serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }

            var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

            if (targetObjectType == null)
            {
                throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
            }

            foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
            {
                var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
                if (fieldInfo != null)
                {
                    return (TAttribute[])fieldInfo.GetCustomAttributes<TAttribute>(inherit);
                }

                var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
                if (propertyInfo != null)
                {
                    return (TAttribute[])propertyInfo.GetCustomAttributes<TAttribute>(inherit);
                }
            }

            throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
        }

    }
}
#endif