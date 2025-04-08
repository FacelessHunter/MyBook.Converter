using DataTranslator.Interfaces;
using Microsoft.Playwright;
using System.Text;

namespace DataTranslator.Implementations
{
    public class DeepLTranslateProvider : ITranslateProvider, IDisposable
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private IBrowserContext _browserContext;
        private const string _baseUrl = "https://www.deepl.com";
        private const int _maxLength = 5000;
        private bool _disposed;

        public async Task InitializeProviderAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 100});
            _browserContext = await _browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize { Width = 2048, Height = 1440 }
            });

            string cookieString = "il=en; __stripe_mid=f2ec2418-69b8-435e-bc9c-d6a371d85fc282a2f1; verifiedBot=false; FPID=FPID2.2.yYIOffyJbuIV0PArL5XgyegOoOXziyuVNd4MBnPzfh0%3D.1741288222; FPLC=%2FSMRBmFKYhnjZh98Z1i8kYKE3AxbAcCAAQKzVJJ7qnMYNy2zGqUBiNpCPD60Bhxq%2Fjb4VUhROb8H7kflEIWabsKQCLlbIVdgmX9P%2FcZXrQev5yyZn%2BufQ1V74wxcLA%3D%3D; dapUid=4e93920f-141b-467f-9ad7-c68a1f925718; privacySettings=%7B%22v%22%3A2.2%2C%22t%22%3A1744269323449%2C%22m%22%3A%22STRICT%22%2C%22consent%22%3A%5B%22NECESSARY%22%2C%22PERFORMANCE%22%2C%22COMFORT%22%2C%22MARKETING%22%5D%7D; _ga=GA1.1.1486031020.1744269324; FPAU=1.2.1838574790.1744269322; __stripe_sid=2397482a-e2e4-4f2e-8806-b43412e945c24b2f59; dapVn=3; _gcl_au=1.1.1091859167.1744270827; _fwb=149KP0U8DJCXMv4A1AM9p8l.1744270827104; __hstc=169224766.f0929a2036359a3a1c07944dd6971812.1744270828057.1744270828057.1744270828057.1; hubspotutk=f0929a2036359a3a1c07944dd6971812; __hssrc=1; __hssc=169224766.1.1744270828058; rskxRunCookie=0; rCookie=qie06lys1wrk60ddhlqrvm9b1wxp9; userCountry=UA; __cf_bm=LPXX2wjMP_o70NDTWTSlwQ4GFsHtUg6fnpg7ubQ9gm4-1744271749-1.0.1.1-Tq5wrl0Lx9.jvme9GlvT1iK2bsXiWKg1JHXTPjFwRgbXpibMa6Ksvdrj.vKPz7rwbV0lSmw.Wn5npXrNKP0R9BC70yCkhYOXnCTtzkVc86E; lastRskxRun=1744272176624; wcs_bt=s_568f0eef8df9:1744272214; dl_session=fw.da19150e-1566-4874-9170-8ed97fbafb9b; LMTBID=v2|e1f72160-5aa3-45b8-8b4b-0bd79b4920bb|f6390cd582ce9506028a703648bd4e52; dapGid=Y0DGb-7Kpi8ZUQctfGayLYkx39kJ9rlxOH4PyQ1N664FFGDZpeDSmbOQ_KCkjWWBkEN82uZIisyExPdSvBGbxMyGRxTfzP5aJdoMIwup1S7VazfsZY1nZ_IL-GS7T7O6wSm8cVUTyaXAHHjh1FA-QsxSk8IOEOUOFHyrcSAQIK72cfnVVhI9zQ; _ga_66CXJP77N5=GS1.1.1744269323.1.1.1744272343.0.0.1685402844; releaseGroups=23217.AAEXP-21144.1.1_18488.DF-4244.2.2_23235.AAEXP-21162.1.1_23181.AAEXP-21108.1.1_13132.DM-1798.2.2_23245.AAEXP-21172.1.1_18131.DM-1931.2.2_15325.DM-1418.2.7_23200.AAEXP-21127.1.1_9824.AP-523.2.3_23185.AAEXP-21112.2.1_23225.AAEXP-21152.1.1_23206.AAEXP-21133.2.1_20742.DF-4301.2.2_21556.DM-2121.2.2_23211.AAEXP-21138.1.1_22982.AAEXP-20918.1.1_23240.AAEXP-21167.1.1_23247.AAEXP-21174.1.1_19673.WTT-1586.2.6_23177.AAEXP-21104.2.1_23219.AAEXP-21146.1.1_13134.DF-3988.2.2_23178.AAEXP-21105.2.1_23251.AAEXP-21178.1.1_23254.AAEXP-21181.1.1_23228.AAEXP-21155.1.1_23234.AAEXP-21161.1.1_17271.DF-4240.2.2_21554.DAL-1566.2.2_23174.AAEXP-21101.1.1_23213.AAEXP-21140.1.1_23224.AAEXP-21151.1.1_23261.AAEXP-21188.1.1_23205.AAEXP-21132.1.1_21298.WTT-1299.2.6_22599.DM-2220.1.1_14958.DF-4137.2.3_23218.AAEXP-21145.1.1_23207.AAEXP-21134.1.1_23183.AAEXP-21110.2.1_21025.CLAB-116.2.3_23227.AAEXP-21154.1.1_23141.TW-452.2.1_23179.AAEXP-21106.2.1_22083.WDW-1120.2.4_23216.AAEXP-21143.1.1_23221.AAEXP-21148.1.1_23208.AAEXP-21135.1.1_22616.DAL-1757.2.2_23222.AAEXP-21149.1.1_23233.AAEXP-21160.1.1_23204.AAEXP-21131.1.1_23232.AAEXP-21159.1.1_23239.AAEXP-21166.2.1_2455.DPAY-2828.2.2_23186.AAEXP-21113.1.1_8776.DM-1442.2.2_23226.AAEXP-21153.1.1_220.DF-1925.1.9_8393.DPAY-3431.2.2_23198.AAEXP-21125.1.1_23135.TW-18.2.2_23209.AAEXP-21136.1.1_20992.TACO-545.2.3_13870.DF-4078.2.2_23249.AAEXP-21176.1.1_14526.RI-246.2.7_23190.AAEXP-21117.2.1_8287.TC-1035.2.5_23237.AAEXP-21164.1.1_23192.AAEXP-21119.1.1_11549.DM-1149.2.2_12645.DAL-1151.2.1_14056.DF-4050.2.2_22359.CORTEX-495.1.6_13564.DF-4046.2.3_16021.DM-1471.2.2_23212.AAEXP-21139.1.1_23196.AAEXP-21123.1.1_23189.AAEXP-21116.2.1_13135.DF-4076.2.2_13872.EXP-133.2.2_23195.AAEXP-21122.1.1_21299.DF-4319.2.4_12498.DM-1867.2.3_22984.AAEXP-20920.1.1_23241.AAEXP-21168.1.1_23250.AAEXP-21177.2.1_21296.DF-4320.2.2_17685.DF-4246.2.2_18116.DF-4250.2.2_23176.AAEXP-21103.2.1_23193.AAEXP-21120.2.1_23243.AAEXP-21170.1.1_23248.AAEXP-21175.1.1_23258.AAEXP-21185.1.1_5030.B2B-444.2.8_21828.CLAB-286.1.4_23214.AAEXP-21141.1.1_22615.B2B-1833.2.2_23220.AAEXP-21147.1.1_14097.DM-1916.2.2_23184.AAEXP-21111.2.1_21302.EXP-282.1.2_12500.DF-3968.2.2_10382.DF-3962.2.2_23175.AAEXP-21102.1.1_23202.AAEXP-21129.1.1_23215.AAEXP-21142.1.1_22071.DF-4338.2.2_22983.AAEXP-20919.2.1_23197.AAEXP-21124.1.1_22087.CEX-1454.2.2_22980.AAEXP-20916.1.1_23238.AAEXP-21165.1.1_23257.AAEXP-21184.1.1_23260.AAEXP-21187.1.1_3961.B2B-663.2.3_23244.AAEXP-21171.1.1_10550.DWFA-884.2.2_19666.DAL-1445.2.2_23203.AAEXP-21130.2.1_21832.TW-472.2.2_23199.AAEXP-21126.1.1_23236.AAEXP-21163.1.1_23229.AAEXP-21156.1.1_22885.CEX-1447.2.3_22981.AAEXP-20917.1.1_23194.AAEXP-21121.2.1_21830.B2B-1758.2.2_23230.AAEXP-21157.1.1_22084.AP-857.1.1_23231.AAEXP-21158.1.1_23259.AAEXP-21186.1.1_23223.AAEXP-21150.1.1_20042.DF-4302.2.3_21557.DM-1889.2.1_22886.CLAB-256.2.2_23252.AAEXP-21179.1.1_23253.AAEXP-21180.1.1_18487.DF-4161.2.4_21301.B2B-1685.2.2_23187.AAEXP-21114.1.1_23246.AAEXP-21173.1.1_18115.DF-4260.2.2_16753.DF-4044.2.3_23242.AAEXP-21169.1.1_23182.AAEXP-21109.1.1_23180.AAEXP-21107.2.1_21297.DM-1605.2.2_23201.AAEXP-21128.1.1_23255.AAEXP-21182.1.1_23256.AAEXP-21183.1.1_23210.AAEXP-21137.1.1_23188.AAEXP-21115.1.1_23191.AAEXP-21118.1.1; _uetsid=91fac0e015db11f0b9802f4c304400cf; _uetvid=91fab8a015db11f0a1b41194f6f37961; dapSid=%7B%22sid%22%3A%22177f6a2f-1dd0-403f-be8e-1e7199f8c3f4%22%2C%22lastUpdate%22%3A1744272502%7D"; // shortened here
            string domain = ".deepl.com";

            var cookies = ParseCookies(cookieString, domain);
            await _browserContext.AddCookiesAsync(cookies.ToArray());

            _page = await _browserContext.NewPageAsync();

            await _page.GotoAsync(_baseUrl);

            // Inject localStorage values
            var localStorageValues = new Dictionary<string, string>
            {
                { "LMT_navigation_sidebar", "{\"isSidebarCollapsed\":\"{\\\"_value\\\":true,\\\"_persistAt\\\":1744269496575}\",\"isOnboardingDismissed\":\"{\\\"_value\\\":true,\\\"_persistAt\\\":1744269496576}\",\"_persist\":\"{\\\"version\\\":1,\\\"rehydrated\\\":true}\"}" },
                
            };

            await SetLocalStorageAsync(_page, localStorageValues);

            // Reload to apply settings
            await _page.ReloadAsync();
        }
        public static IEnumerable<Microsoft.Playwright.Cookie> ParseCookies(string rawCookieString, string domain)
        {
            var cookiePairs = rawCookieString.Split(';');
            foreach (var pair in cookiePairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length != 2) continue;

                yield return new Microsoft.Playwright.Cookie
                {
                    Name = parts[0].Trim(),
                    Value = parts[1].Trim(),
                    Domain = domain,
                    Path = "/",
                    HttpOnly = false,
                    Secure = true
                };
            }
        }
        public async Task SetLocalStorageAsync(IPage page, Dictionary<string, string> localStorageValues)
        {
            foreach (var item in localStorageValues)
            {
                string script = $"localStorage.setItem({EscapeForJs(item.Key)}, {EscapeForJs(item.Value)});";
                await page.EvaluateAsync(script);
            }
        }

        private string EscapeForJs(string input)
        {
            return System.Text.Json.JsonSerializer.Serialize(input); // handles escaping for quotes, etc.
        }

        public async Task<string> TranslateAsync(string text, string fromLang = "en", string toLang = "uk", string separationSequence = null)
        {
            var isExceedingLimit = text.Length >= _maxLength;

            if (isExceedingLimit && separationSequence is null)
                throw new Exception("Out of limit: 5000");

            if (isExceedingLimit) 
            {
                var chunks = SplitIntoChunks(text, separationSequence, _maxLength);

                List<string> result = new List<string>();
                var i = 0;

                foreach (var chunk in chunks)
                {
                    i++;

                    string chunkOutput = await InternalTranslate(chunk, fromLang, toLang);

                    result.Add(chunkOutput);

                    Console.WriteLine($"Translated chunk({i}) of chunks({chunks.Count}) \n Original Text: {chunk[0..10]} \n Translated Text: {chunkOutput[0..10]}");

                }
                return string.Join(separationSequence, result);
            }

            string output = await InternalTranslate(text, fromLang, toLang);

            Console.WriteLine("Text is translated ");

            return output;
        }

        private async Task<string> InternalTranslate(string text, string fromLang, string toLang)
        {

            string url = $"{_baseUrl}/translator#{fromLang}/{toLang}/ ";
            await _page.GotoAsync(url);
            await Task.Delay(1000);

            //await SelectLanguage(fromLang, toLang);

            var sourceLocator = _page.Locator("//div[@aria-labelledby=\"translation-source-heading\"]");
            await sourceLocator.FillAsync(text);

            var translatedLocator = _page.Locator("//div[@aria-labelledby=\"translation-target-heading\"]");
            await translatedLocator.WaitForAsync();

            const int safeTimerLimit = 10;
            string output;
            var safeTimer = 0;
            var failureCondition = false;
            do
            {
                await Task.Delay(3000);

                output = await translatedLocator.InnerTextAsync();

                safeTimer++;

                failureCondition = (string.IsNullOrEmpty(output) || output == "\n" || output.Contains("[...]")) && !(safeTimer > safeTimerLimit);

            }
            while (failureCondition);

            if (failureCondition)
                Console.WriteLine("Failed to Load: " + output);

            //Do force refresh
            if (failureCondition)
            {

                Console.WriteLine("Do force refresh");
                return await InternalTranslate(text, fromLang, toLang);
            }

            return output;
        }

        private async Task SelectLanguage(string fromLang, string toLang)
        {
            var targetLanguageSelectorLocator = _page.Locator("//button[@data-testid=\"translator-target-lang-btn\"]/parent::span");
            await targetLanguageSelectorLocator.ClickAsync();

            //await _page.EvaluateAsync(
            //    " () => {" +
            //    "const input = document.querySelector('//input[@id=\"search-bar-id-:r6h:\"]');" +
            //    "input.value = 'my value';" +
            //    "input.dispatchEvent(new Event('input', { bubbles: true }));" +
            //   " input.dispatchEvent(new Event('change', { bubbles: true }));" +
            //    "}"
            //   );

            var targetLanguageSelectorInputLocator = _page.Locator("//input[@placeholder=\"Search languages\"]");
            await targetLanguageSelectorInputLocator.FillAsync(toLang);

            var targetLanguageSelectorProposedLocator = _page.Locator($"//button[@data-testid=\"translator-lang-option-{toLang}\"]");
            await targetLanguageSelectorProposedLocator.ClickAsync();
        }

        private static List<string> SplitIntoChunks(string input, string separationSequence, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            var chunks = new List<string>();
            var parts = input.Split(new[] { separationSequence }, StringSplitOptions.None);

            var currentChunk = new StringBuilder();

            foreach (var part in parts)
            {
                string candidate = currentChunk.Length > 0
                    ? currentChunk + separationSequence + part
                    : part;

                if (candidate.Length > maxLength)
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }

                    // If the individual part itself is longer than maxLength,
                    // split it hard (no separationSequence).
                    if (part.Length > maxLength)
                    {
                        for (int i = 0; i < part.Length; i += maxLength)
                        {
                            int len = Math.Min(maxLength, part.Length - i);
                            chunks.Add(part.Substring(i, len));
                        }
                    }
                    else
                    {
                        currentChunk.Append(part);
                    }
                }
                else
                {
                    if (currentChunk.Length > 0)
                        currentChunk.Append(separationSequence);

                    currentChunk.Append(part);
                }
            }

            if (currentChunk.Length > 0)
                chunks.Add(currentChunk.ToString().Trim());

            return chunks;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _playwright?.Dispose();
                }

                _disposed = true;
            }
        }

    }
}
