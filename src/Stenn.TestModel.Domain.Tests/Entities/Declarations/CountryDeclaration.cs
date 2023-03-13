#nullable enable

using System;
using System.Collections.Generic;
using Stenn.Csv;

namespace Stenn.TestModel.Domain.Tests.Entities.Declarations
{
    public static class CountryDeclaration
    {
        internal static readonly string UsaId = "US";

        public static IEnumerable<Country> GetActual()
        {
            //NOTE: Generate new db migration after change csv file

            return CsvConverter.ReadCsvFromResFile<CountryData, Country>(@"Entities\Declarations\CountryDeclaration.csv", data =>
                new Country(data.Alpha2Code,
                    data.Name,
                    CsvConverter.NullOrEmpty(data.Alpha3Code),
                    CsvConverter.NullOrEmpty(data.Numeric3Code),
                    data.NominalGnp ?? 0,
                    data.Created ?? DateTime.UtcNow)
                {
                    IsInEuropeanUnion = CsvConverter.Bool(data.IsInEuropeanUnion),
                    FlagId = data.FlagId,
                    PhoneCode = data.PhoneCode,
                    AlternativeNames = CsvConverter.NullOrEmpty(data.AlternativeNames),
                });
        }

#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        private sealed class CountryData
        {
            public string? Name { get; set; }
            public string? Alpha2Code { get; set; }
            public string? Alpha3Code { get; set; }
            public string? Numeric3Code { get; set; }
            public string? IsInEuropeanUnion { get; set; }
            public string? FlagId { get; set; }
            public string? PhoneCode { get; set; }
            public string? AlternativeNames { get; set; }
            public decimal? NominalGnp { get; set; }
            public DateTime? Created { get; set; }
        }
    }
}