using System;
using System.Collections.Generic;
using static bottlenoselabs.Interop.SDL;

namespace IDE;

public unsafe class TextureManager : IDisposable
{
    private readonly SDL_Renderer* _renderer;
    private readonly Dictionary<string, nint> _textures = new();

    public TextureManager(SDL_Renderer* renderer)
    {
        _renderer = renderer;
    }

    public nint GetTexture(string path)
    {
        if (_textures.TryGetValue(path, out var texture))
            return texture;

        fixed (byte* pPath = System.Text.Encoding.UTF8.GetBytes(path + '\0'))
        {
            var cpath = new Interop.Runtime.CString(pPath);
            var surface = bottlenoselabs.Interop.SDL_image.IMG_Load(cpath);
            if (surface == null)
                return 0;

            var sdlTex = SDL_CreateTextureFromSurface(_renderer, surface);
            SDL_DestroySurface(surface);

            if (sdlTex == null)
                return 0;

            _textures[path] = (nint)sdlTex;
            return (nint)sdlTex;
        }
    }

    public void Dispose()
    {
        foreach (var tex in _textures.Values)
        {
            SDL_DestroyTexture((SDL_Texture*)tex);
        }
        _textures.Clear();
    }
}
