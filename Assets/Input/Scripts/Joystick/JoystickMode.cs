namespace AngryKoala.Input
{
    public enum JoystickMode
    {
        None,

        /// <summary>
        /// The joystick stays at its place in the area.
        /// </summary>
        Fixed,

        /// <summary>
        /// The joystick appears where the finger goes down and stays there until it lifts.
        /// </summary>
        Floating,

        /// <summary>
        /// As floating, and the joystick is pulled along when the finger moves further than the radius.
        /// </summary>
        Following
    }
}
