/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;
using OpenSim.Tests.Common;

namespace OpenSim.Capabilities.Handlers.Tests
{
    public class MockAssetService : IAssetService
    {
        private AssetBase m_asset;

        public MockAssetService(AssetBase asset)
        {
            m_asset = asset;
        }

        public AssetBase Get(string id)
        {
            if (m_asset != null && m_asset.ID == id)
                return m_asset;
            return null;
        }

        public AssetMetadata GetMetadata(string id) { return null; }
        public byte[] GetData(string id) { return null; }
        public string Store(AssetBase asset) { return asset.ID; }
        public string UpdateContent(UUID id, byte[] data) { return id.ToString(); }
        public bool Delete(UUID id) { return true; }
    }

    public class TestGetTextureRobustHandler : GetTextureRobustHandler
    {
        public TestGetTextureRobustHandler(string path, IAssetService assService, string name, string description, string redirectURL)
            : base(path, assService, name, description, redirectURL)
        {
        }

        public byte[] PublicProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            return base.ProcessRequest(path, request, httpRequest, httpResponse);
        }
    }

    [TestFixture]
    public class GetTextureRobustHandlerTests
    {
        [Test]
        public void TestSuffixRange()
        {
            // 1. Setup
            UUID textureID = UUID.Random();
            byte[] textureData = new byte[1000];
            new Random().NextBytes(textureData);

            AssetBase textureAsset = new AssetBase(textureID, "test-texture", (sbyte)AssetType.Texture, UUID.Zero.ToString());
            textureAsset.Data = textureData;
            textureAsset.Metadata.ContentType = "image/x-j2c";

            IAssetService assetService = new MockAssetService(textureAsset);
            var handler = new TestGetTextureRobustHandler("/texture", assetService, "GetTexture", "", null);

            var request = new MockHttpRequest();
            request.Url = new Uri("http://localhost:8002/texture?texture_id=" + textureID.ToString());
            request.AddHeader("Range", "bytes=-500");

            var response = new MockHttpResponse();

            // 2. Act
            handler.PublicProcessRequest("/texture?texture_id=" + textureID.ToString(), null, request, response);

            // 3. Assert
            Assert.AreEqual((int)HttpStatusCode.PartialContent, response.StatusCode);
            Assert.AreEqual(500, response.ContentLength);
            Assert.AreEqual(500, response.RawBufferStart);
            Assert.AreEqual(500, response.RawBufferLen);
            Assert.AreEqual("bytes 500-999/1000", response.Headers["Content-Range"]);
        }
    }
}
