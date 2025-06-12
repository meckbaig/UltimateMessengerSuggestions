using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json;

namespace UltimateMessengerSuggestions.Common.Conventions;

/// <summary>  
/// Converts query parameter names to camelCase for API requests.  
/// </summary>  
public class CamelCaseQueryParameterConvention : IParameterModelConvention
{
    /// <summary>  
    /// Applies the camelCase naming convention to query parameter names.  
    /// </summary>  
    /// <param name="parameter"></param>  
    public void Apply(ParameterModel parameter)
    {
        if (parameter.BindingInfo?.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Query)
        {
            var originalName = parameter.ParameterName;
            var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(originalName);

            // Fix for CS8858: Directly set the BinderModelName property instead of using 'with' syntax.  
            if (parameter.BindingInfo != null)
            {
                parameter.BindingInfo.BinderModelName = camelCaseName;
            }
        }
    }
}
