using AutoMapper.Execution;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Books.Api
{
    // custom model binder
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // our binder works only on enumerable types
            if(!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Get the inputted value through the value provider
            // this line gives us a string with a list of guids seperated by commas
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            if(string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // the value is not null or whitespace
            // and the type of the model is enumerable.
            // get the enumerable's type, and a converter
            // the elementType will be GUID
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];

            // we need to convert each guid in the string to an actual guid
            var converter = TypeDescriptor.GetConverter(elementType);

            // Convert each item in the value list to the enumerable type
            // now we have list of guids, but it is not typed list yet
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim())).ToArray();

            // create an array of the type, and set it as the model value
            // so now we converted list of strings to array of GUIDS
            var typesValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typesValues, 0);
            bindingContext.Model = typesValues;

            // return a successful result, passing in the model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;

        }
    }
}
