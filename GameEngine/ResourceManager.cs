using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Audio;

namespace GameEngine
{
    public class ResourceManager
    {
        private static ResourceManager instance = null;
        Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        Dictionary<string, SpriteSheet> _spriteSheets = new Dictionary<string, SpriteSheet>();
        Dictionary<string, Sound> _sounds = new Dictionary<string, Sound>();
        Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        //cria singleton
        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourceManager();
                }
                return instance;
            }
        }

        public void LoadTextureFromFile(string name, string path)
        {
            if(GetTexture(name) == null)    //se não contém a textura na tabela de recursos
            { 
                Texture texture = new Texture(path);    //cria textura
                _textures.Add(name, texture);           //adiciona textura a tabela
            }
        }

        public Texture GetTexture(string name)
        {
            if (_textures.ContainsKey(name))    //se a chave da textura está presente na tabela
                return _textures[name];         //devolve a textura
            else return null;                   //caso contrário, retorna null
        }

        public void LoadSpriteSheetFromFile(string name, string path, int totalFrames)
        {
            if (!_spriteSheets.ContainsKey(name))   //se a spritesheet não estiver na tabela
            {
                SpriteSheet spriteSheet = new SpriteSheet();    //cria nova spritesheet
                spriteSheet.texture = new Texture(path);        //cria nova textura para spritesheet
                spriteSheet.TotalFrames = totalFrames;          //informa quantidade de frames da spritesheet

                _spriteSheets.Add(name, spriteSheet);           //adiciona a spritesheet a tabela
            }
        }

        public SpriteSheet GetSpriteSheet(string name)
        {
            if (_spriteSheets.ContainsKey(name))        //se a chave da spritesheet está na tabela
                return _spriteSheets[name];             //entrega a spritesheet
            else return null;                           //caso coontrário, retorna null
        }


        public bool LoadSoundFromFile(string name, string path)
        {
            if (GetSound(name) == null)                 //se o som não estiver na tabela
            {
                SoundBuffer _soundBuffer = new SoundBuffer(path);   //cria novo som
                Sound s = new Sound(_soundBuffer);                  //adequa ao formato da tabela
                _sounds.Add(name, s);                               //coloca na tabela
                return true;                                        //retorna true pq sim
            }
            return true;
        }

        public bool LoadFontFromFile(string name, string path)
        {
            Font font = new Font(path);     //cria nova fonte
            _fonts.Add(name, font);         //coloca na tabela

            return true;

        }


        public Sound GetSound(string name)
        {
            if (_sounds.ContainsKey(name))  //se o som está na tabela
                return _sounds[name];       //retorna o som
            else return null;               //caso contrário, retorna null
        }

        public void PlaySound(string name)
        {
            if (_sounds.ContainsKey(name))  //se o som está na tabela
                _sounds[name].Play();       //toca
        }

        public void StopSound(string name)
        {
            if (_sounds.ContainsKey(name))  //se o som está na tabela
                _sounds[name].Stop();       //para
        }

        public SoundStatus GetSoundStatus(string name)
        {
            if (_sounds.ContainsKey(name))  //se o som esta na tabela
                return _sounds[name].Status;//retorna o status atual (imagino que pode inclusive ser Stopped)
            else
                return SoundStatus.Stopped; //caso não esteja, retorna Stopped
        }

        public Font GetFont(string name)
        {
            if (_fonts.ContainsKey(name))   //se a fonte está na tabela
                return _fonts[name];        //retorna a fonte
            else
                return null;                //caso contrario, retorna null
        }


    }
}
