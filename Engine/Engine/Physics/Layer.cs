namespace SE.Physics
{

    /// <summary>
    /// Layer used for the physics system. Controls which physics objects can interact with each other by assigning them to layers.
    /// </summary>
    public class Layer
    {
        /// <summary>Which other physics layers this layer can interact with.</summary>
        public LayerType[] Interactions;

        /// <summary>
        /// Checks if this physics layer colliders with another.
        /// </summary>
        /// <param name="layerType">Other layer to check interactions with.</param>
        /// <returns>True if both layers interact with each other.</returns>
        public bool CollidesWith(LayerType layerType)
        {
            for (int i = 0; i < Interactions.Length; i++) {
                if (Interactions[i] == layerType) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a new physics layer instance.
        /// </summary>
        /// <param name="interactions">Which physics layers to interact with.</param>
        public Layer(params LayerType[] interactions)
        {
            Interactions = new LayerType[interactions.Length];
            for (int i = 0; i < interactions.Length; i++) {
                Interactions[i] = interactions[i];
            }
        }

        /// <summary>
        /// Returns all physics layers.
        /// </summary>
        public static LayerType[] All => new[] { LayerType.Player, LayerType.Enemy, LayerType.MapTile, LayerType.Glass, LayerType.PlayerBullet, LayerType.EnemyBullet };

    }

}
