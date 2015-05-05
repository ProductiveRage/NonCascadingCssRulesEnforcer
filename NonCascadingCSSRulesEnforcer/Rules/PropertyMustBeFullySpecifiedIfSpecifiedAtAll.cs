using System;
using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules
{
    /// <summary>
    /// This performs the shared work of the BorderWidthMustBeFullySpecifiedIfSpecifiedAtAll, MarginMustBeFullySpecifiedIfSpecifiedAtAll and PaddingMustBeFullySpecifiedIfSpecifiedAtAll
    /// rules - it is used to ensure that none of these measurements are partially specified (eg. "padding-left" may not appear on its own, either all padding values must be specified
    /// with the use of "padding" and then the "padding-left" being used to override that, or the "padding-top", "padding-bottom" and "padding-right" properties must also be present)
    /// </summary>
    public abstract class PropertyMustBeFullySpecifiedIfSpecifiedAtAll : IEnforceRules
    {
        private readonly HashSet<string> _topSideProperties, _leftSideProperties, _bottomSideProperties, _rightSideProperties, _allSidesProperties;
        private readonly Func<ICSSFragment, BrokenRuleEncounteredException> _exceptionRaiser;
        protected PropertyMustBeFullySpecifiedIfSpecifiedAtAll(
            IEnumerable<string> topSideProperties,
            IEnumerable<string> leftSideProperties,
            IEnumerable<string> bottomSideProperties,
            IEnumerable<string> rightSideProperties,
            IEnumerable<string> allSidesProperties,
            Func<ICSSFragment, BrokenRuleEncounteredException> exceptionRaiser)
        {
            if (topSideProperties == null)
                throw new ArgumentNullException("topSideProperties");
            if (leftSideProperties == null)
                throw new ArgumentNullException("leftSideProperties");
            if (bottomSideProperties == null)
                throw new ArgumentNullException("bottomSideProperties");
            if (rightSideProperties == null)
                throw new ArgumentNullException("rightSideProperties");
            if (allSidesProperties == null)
                throw new ArgumentNullException("allSidesProperties");
            if (exceptionRaiser == null)
                throw new ArgumentNullException("exceptionRaiser");

            try { _topSideProperties = ToNonNullOrEmptyHashSet(topSideProperties, StringComparer.InvariantCultureIgnoreCase); }
            catch (Exception e) { throw new ArgumentException("Invalid topSideProperties", e); }

            try { _leftSideProperties = ToNonNullOrEmptyHashSet(leftSideProperties, StringComparer.InvariantCultureIgnoreCase); }
            catch (Exception e) { throw new ArgumentException("Invalid leftSideProperties", e); }

            try { _bottomSideProperties = ToNonNullOrEmptyHashSet(bottomSideProperties, StringComparer.InvariantCultureIgnoreCase); }
            catch (Exception e) { throw new ArgumentException("Invalid bottomSideProperties", e); }

            try { _rightSideProperties = ToNonNullOrEmptyHashSet(rightSideProperties, StringComparer.InvariantCultureIgnoreCase); }
            catch (Exception e) { throw new ArgumentException("Invalid rightSideProperties", e); }

            try { _allSidesProperties = ToNonNullOrEmptyHashSet(allSidesProperties, StringComparer.InvariantCultureIgnoreCase); }
            catch (Exception e) { throw new ArgumentException("Invalid allSidesProperties", e); }

            _exceptionRaiser = exceptionRaiser;
        }

        /// <summary>
        /// This will never return null nor a set containing any null or blank values, nor any with leading or trailing whitespace
        /// </summary>
        private HashSet<string> ToNonNullOrEmptyHashSet(IEnumerable<string> values, IEqualityComparer<string> comparer)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var valuesTidied = values.ToArray();
            if (valuesTidied.Any(v => string.IsNullOrWhiteSpace(v)))
                throw new ArgumentException("Null/blank value encountered");
            return new HashSet<string>(values.Select(v => v.Trim()), comparer);
        }

        public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
        {
            if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
                throw new ArgumentOutOfRangeException("styleSheetType");

            return (styleSheetType != StyleSheetTypeOptions.Compiled && styleSheetType != StyleSheetTypeOptions.Combined);
        }

        /// <summary>
        /// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
        /// references, or one which contains a null reference.
        /// </summary>
        public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
        {
            if (fragments == null)
                throw new ArgumentNullException("fragments");

            var firstBrokenRuleIfAny = GetAnyBrokenRules(fragments).FirstOrDefault();
            if (firstBrokenRuleIfAny != null)
                throw firstBrokenRuleIfAny;
        }

        public IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments)
        {
            if (fragments == null)
                throw new ArgumentNullException("fragments");

            foreach (var fragment in fragments)
            {
                var containerFragment = fragment as ContainerFragment;
                if (containerFragment == null)
                    continue;

                // We'll be looping through this multiple times so ToArray is called to ensure it's only evaluated once
                var stylePropertyNames = containerFragment.ChildFragments.Where(f => f is StylePropertyName).Cast<StylePropertyName>().Select(s => s.Value.ToLower()).ToArray();

                // If at least one side is explicitly specified then ensure that all sides have a value
                var topSideExplicitlySet = stylePropertyNames.Any(s => _topSideProperties.Contains(s));
                var leftSideExplicitlySet = stylePropertyNames.Any(s => _leftSideProperties.Contains(s));
                var bottomSideExplicitlySet = stylePropertyNames.Any(s => _bottomSideProperties.Contains(s));
                var rightSideExplicitlySet = stylePropertyNames.Any(s => _rightSideProperties.Contains(s));
                if (topSideExplicitlySet || leftSideExplicitlySet || bottomSideExplicitlySet || rightSideExplicitlySet)
                {
                    var allSidesSet =
                        stylePropertyNames.Any(s => _allSidesProperties.Contains(s)) ||
                        (topSideExplicitlySet && leftSideExplicitlySet && bottomSideExplicitlySet && rightSideExplicitlySet);
                    if (!allSidesSet)
                        yield return _exceptionRaiser(containerFragment);
                }

                foreach (var brokenRule in GetAnyBrokenRules(containerFragment.ChildFragments))
                    yield return brokenRule;
            }
        }
    }
}
