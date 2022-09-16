using SdlDotNet.Input;

namespace ZXMAK.Platform.SDL
{
    public class Keyboard
    {
        private long _state;
        private KeyboardState _keyboardState;

        public Keyboard() => this._keyboardState = new KeyboardState();

        public void Scan()
        {
            this._state = 0L;
            this._keyboardState.Update();
            this._state = this.parseKeyboardState(this._keyboardState);
        }

        public long State => this._state;

        private long parseKeyboardState(KeyboardState state)
        {
            long num = 0;
            return state[Key.LeftAlt] || state[Key.RightAlt] ? 0L : (((((((num << 5 | (long)Keyboard.parse_7FFE(state)) << 5 | (long)Keyboard.parse_BFFE(state)) << 5 | (long)Keyboard.parse_DFFE(state)) << 5 | (long)Keyboard.parse_EFFE(state)) << 5 | (long)Keyboard.parse_F7FE(state)) << 5 | (long)Keyboard.parse_FBFE(state)) << 5 | (long)Keyboard.parse_FDFE(state)) << 5 | (long)Keyboard.parse_FEFE(state);
        }

        private static byte parse_7FFE(KeyboardState state)
        {
            byte num = 0;
            if (state[Key.Space])
                num |= (byte)1;
            if (state[Key.RightShift])
                num |= (byte)2;
            if (state[Key.M])
                num |= (byte)4;
            if (state[Key.N])
                num |= (byte)8;
            if (state[Key.B])
                num |= (byte)16;
            if (state[Key.CapsLock] || state[Key.KeypadPlus] || state[Key.KeypadMinus] || state[Key.KeypadMultiply] || state[Key.KeypadDivide])
                num |= (byte)2;
            if (state[Key.Period] || state[Key.Comma] || state[Key.Semicolon] || state[Key.Quote] || state[Key.Slash] || state[Key.Minus] || state[Key.Equals] || state[Key.LeftBracket] || state[Key.RightBracket])
                num |= (byte)2;
            if (state[Key.KeypadMultiply])
                num |= (byte)16;
            if (!state[Key.RightShift])
            {
                if (state[Key.Period])
                    num |= (byte)4;
                if (state[Key.Comma])
                    num |= (byte)8;
            }
            return num;
        }

        private static byte parse_BFFE(KeyboardState state)
        {
            byte bffe = 0;
            if (state[Key.Return])
                bffe |= (byte)1;
            if (state[Key.L])
                bffe |= (byte)2;
            if (state[Key.K])
                bffe |= (byte)4;
            if (state[Key.J])
                bffe |= (byte)8;
            if (state[Key.H])
                bffe |= (byte)16;
            if (state[Key.KeypadEnter])
                bffe |= (byte)1;
            if (state[Key.KeypadMinus])
                bffe |= (byte)8;
            if (state[Key.KeypadPlus])
                bffe |= (byte)4;
            if (state[Key.RightShift])
            {
                if (state[Key.Equals])
                    bffe |= (byte)4;
            }
            else
            {
                if (state[Key.Minus])
                    bffe |= (byte)8;
                if (state[Key.Equals])
                    bffe |= (byte)2;
            }
            return bffe;
        }

        private static byte parse_DFFE(KeyboardState state)
        {
            byte dffe = 0;
            if (state[Key.P])
                dffe |= (byte)1;
            if (state[Key.O])
                dffe |= (byte)2;
            if (state[Key.I])
                dffe |= (byte)4;
            if (state[Key.U])
                dffe |= (byte)8;
            if (state[Key.Y])
                dffe |= (byte)16;
            if (state[Key.RightShift])
            {
                if (state[Key.Quote])
                    dffe |= (byte)1;
            }
            else if (state[Key.Semicolon])
                dffe |= (byte)2;
            return dffe;
        }

        private static byte parse_EFFE(KeyboardState state)
        {
            byte effe = 0;
            if (state[Key.Zero])
                effe |= (byte)1;
            if (state[Key.Nine])
                effe |= (byte)2;
            if (state[Key.Eight])
                effe |= (byte)4;
            if (state[Key.Seven])
                effe |= (byte)8;
            if (state[Key.Six])
                effe |= (byte)16;
            if (state[Key.RightArrow])
                effe |= (byte)4;
            if (state[Key.UpArrow])
                effe |= (byte)8;
            if (state[Key.DownArrow])
                effe |= (byte)16;
            if (state[Key.Backspace])
                effe |= (byte)1;
            if (state[Key.RightShift])
            {
                if (state[Key.Minus])
                    effe |= (byte)1;
            }
            else if (state[Key.Quote])
                effe |= (byte)8;
            return effe;
        }

        private static byte parse_F7FE(KeyboardState state)
        {
            byte f7Fe = 0;
            if (state[Key.One])
                f7Fe |= (byte)1;
            if (state[Key.Two])
                f7Fe |= (byte)2;
            if (state[Key.Three])
                f7Fe |= (byte)4;
            if (state[Key.Four])
                f7Fe |= (byte)8;
            if (state[Key.Five])
                f7Fe |= (byte)16;
            if (state[Key.LeftArrow])
                f7Fe |= (byte)16;
            return f7Fe;
        }

        private static byte parse_FBFE(KeyboardState state)
        {
            byte fbfe = 0;
            if (state[Key.Q])
                fbfe |= (byte)1;
            if (state[Key.W])
                fbfe |= (byte)2;
            if (state[Key.E])
                fbfe |= (byte)4;
            if (state[Key.R])
                fbfe |= (byte)8;
            if (state[Key.T])
                fbfe |= (byte)16;
            if (state[Key.RightShift])
            {
                if (state[Key.Period])
                    fbfe |= (byte)16;
                if (state[Key.Comma])
                    fbfe |= (byte)8;
            }
            return fbfe;
        }

        private static byte parse_FDFE(KeyboardState state)
        {
            byte fdfe = 0;
            if (state[Key.A])
                fdfe |= (byte)1;
            if (state[Key.S])
                fdfe |= (byte)2;
            if (state[Key.D])
                fdfe |= (byte)4;
            if (state[Key.F])
                fdfe |= (byte)8;
            if (state[Key.G])
                fdfe |= (byte)16;
            return fdfe;
        }

        private static byte parse_FEFE(KeyboardState state)
        {
            byte fefe = 0;
            if (state[Key.LeftShift])
                fefe |= (byte)1;
            if (state[Key.Z])
                fefe |= (byte)2;
            if (state[Key.X])
                fefe |= (byte)4;
            if (state[Key.C])
                fefe |= (byte)8;
            if (state[Key.V])
                fefe |= (byte)16;
            if (state[Key.LeftArrow])
                fefe |= (byte)1;
            if (state[Key.RightArrow])
                fefe |= (byte)1;
            if (state[Key.UpArrow])
                fefe |= (byte)1;
            if (state[Key.DownArrow])
                fefe |= (byte)1;
            if (state[Key.Backspace])
                fefe |= (byte)1;
            if (state[Key.CapsLock])
                fefe |= (byte)1;
            if (state[Key.KeypadDivide])
                fefe |= (byte)16;
            if (state[Key.RightShift])
            {
                if (state[Key.Semicolon])
                    fefe |= (byte)2;
                if (state[Key.Slash])
                    fefe |= (byte)8;
            }
            else if (state[Key.Slash])
                fefe |= (byte)16;
            return fefe;
        }
    }
}
