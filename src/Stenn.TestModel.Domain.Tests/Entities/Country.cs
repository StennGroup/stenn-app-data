using System;

namespace Stenn.TestModel.Domain.Tests.Entities
{
#if NET6_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors)]
#endif
    public class Country
    {
        /// <summary>
        /// Ctor for EF proxies
        /// </summary>
        protected Country()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="alpha3Code"></param>
        /// <param name="numeric3Code"></param>
        /// <param name="nominalGnp"></param>
        public Country(string id, string name, string alpha3Code, string numeric3Code, decimal nominalGnp, DateTime created)
        {
            Id = id;
            Name = name;
            Alpha3Code = alpha3Code;
            Numeric3Code = numeric3Code;
            NominalGnp = nominalGnp;
            Created = created;
        }

        /// <summary>
        /// Alpha2Code. From ISO 3166 international standard
        /// </summary>
        public string Id { get; internal set; }

        public string Name { get; internal set; }

        /// <summary>
        /// From ISO 3166 international standard
        /// </summary>
        public string Alpha3Code { get; init; }

        /// <summary>
        /// From ISO 3166 international standard.
        /// </summary>
        public string Numeric3Code { get; init; }

        /// <summary>
        /// Is country in EU
        /// </summary>
        public bool IsInEuropeanUnion { get; init; }

        /// <summary>
        /// Flag Id in Unicode
        /// </summary>
        public string FlagId { get; init; }

        /// <summary>
        /// Country's phone code starting with +(eg. +381)
        /// </summary>
        public string PhoneCode { get; init; }

        /// <summary>
        /// Alternative country's names. Semicolon separated
        /// </summary>
        public string AlternativeNames { get; init; }

        /// <summary>
        /// Country's nominal GNP
        /// </summary>
        public decimal NominalGnp { get; set; }

        /// <summary>
        /// Country's record creation date
        /// </summary>
        public DateTime Created { get; set; }
    }
}