using Xunit.Sdk;

namespace OpenApiServer.UnitTests.Utils
{
    public static class DeepAssert
    {
        /// <summary>
        /// Asserts that two objects are equal.
        /// Uses reflection to compare members.
        /// No Equals override required.
        /// </summary>
        /// <typeparam name="T">Objects type.</typeparam>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <exception cref="XunitException"></exception>
        /// <remarks>
        /// This comparison method should not be used in production code,
        /// because of the performance hit caused by reflection.
        /// </remarks>
        /// <remarks>
        /// Depends on Compare-Net-Objects.
        /// See https://github.com/GregFinzer/Compare-Net-Objects
        /// </remarks>
        public static void Equal<T>(T expected, T actual)
        {
            var compareLogic = new KellermanSoftware.CompareNetObjects.CompareLogic();
            var result = compareLogic.Compare(expected, actual);
            if (result.AreEqual)
            {
                return;
            }
            throw new XunitException(result.DifferencesString);
        }
    }
}