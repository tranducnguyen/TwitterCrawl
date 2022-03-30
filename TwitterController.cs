using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Pwpf
{
    class TwitterController
    {
        public void GetListUserName(Info item)
        {
            try
            {
                bool isFirst = true;
                bool isFinish = false;

                if (!item._COOKIE.Contains("ct0"))
                {
                    item._STATUS = "Cookie sai định dạng";
                    updateList();
                    return;
                }
                RestClient client = new RestClient();
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36";
                client.CookieContainer = new CookieContainer();
                client.AddDefaultHeader("Cookie", item._COOKIE);
                string csft = Regex.Match(item._COOKIE, "ct0=(.*?);").Groups[1].Value;

                client.AddDefaultHeader("x-csrf-token", csft);
                client.AddDefaultHeader("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");

                string quoteId = Regex.Match(item._LINK, "status/(\\d+)").Groups[1].Value;
                if (string.IsNullOrEmpty(quoteId))
                {
                    item._STATUS = "Không lấy được quoteID";
                    updateList();
                    return;
                }
                string url;
                string cursor;
                isFinish = false;
                isFirst = true;
                while (!isFinish)
                {
                    url = "";
                    cursor = "";
                    if (isFirst)
                    {
                        url = $"https://twitter.com/i/api/2/search/adaptive.json?include_profile_interstitial_type=1&include_blocking=1&include_blocked_by=1&include_followed_by=1&include_want_retweets=1&include_mute_edge=1&include_can_dm=1&include_can_media_tag=1&skip_status=1&cards_platform=Web-12&include_cards=1&include_ext_alt_text=true&include_quote_count=true&include_reply_count=1&tweet_mode=extended&include_entities=true&include_user_entities=true&include_ext_media_color=true&include_ext_media_availability=true&include_ext_sensitive_media_warning=true&send_error_codes=true&=true&q=quoted_tweet_id%3A{quoteId}&vertical=tweet_detail_quote&count=20&pc=1&spelling_corrections=1&include_ext_has_nft_avatar=false&ext=mediaStats%2ChighlightedLabel%2CvoiceInfo%2CsuperFollowMetadata";
                        isFirst = false;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item._CURSOR))
                        {
                            item._STATUS = "Sleeping.." + CONSTVAL.TimeDelay + "s";
                            updateList();
                            Thread.Sleep(CONSTVAL.TimeDelay * 1000);
                            url = $"https://twitter.com/i/api/2/search/adaptive.json?include_profile_interstitial_type=1&include_blocking=1&include_blocked_by=1&include_followed_by=1&include_want_retweets=1&include_mute_edge=1&include_can_dm=1&include_can_media_tag=1&include_ext_has_nft_avatar=1&skip_status=1&cards_platform=Web-12&include_cards=1&include_ext_alt_text=true&include_quote_count=true&include_reply_count=1&tweet_mode=extended&include_entities=true&include_user_entities=true&include_ext_media_color=true&include_ext_media_availability=true&include_ext_sensitive_media_warning=true&send_error_codes=true&simple_quoted_tweet=true&q=quoted_tweet_id%3A{quoteId}&vertical=tweet_detail_quote&count=20&cursor=scroll%3A{item._CURSOR}&pc=1&spelling_corrections=1&ext=mediaStats%2ChighlightedLabel%2CvoiceInfo%2CsuperFollowMetadata";
                        }
                        else
                        {
                            Debug.Print("Không tìm thấy cursor");
                            return;
                        }

                    }

                    RestRequest request = new RestRequest(url, Method.GET);
                    var resp = client.Execute(request);
                    if (string.IsNullOrEmpty(resp.Content))
                    {
                        item._STATUS = "Không lấy được nội dung";
                        break;
                    }
                    cursor = getLinkRetwit(resp.Content, Directory.GetCurrentDirectory() + "\\output\\" + quoteId + ".txt", item);
                    if (string.IsNullOrEmpty(cursor))
                    {
                        break;
                    }

                    if (cursor == item._CURSOR)
                    {
                        isFinish = true;
                    }
                    else
                    {
                        item._CURSOR = cursor;
                    }
                    item._STATUS = "Đã lấy được " + item._COUNT_LINK + " links";
                    updateList();
                    Thread.Sleep(1000);
                }
            }
            catch { }
        }
        public void updateList()
        {
            Pwpf.MainWindow.main.Refresh();
        }

        string getLinkRetwit(string sjson, string pathFile, Info info)
        {            //string s = File.ReadAllText("json.txt");

            try
            {
                string cursor = "";
                Root data = JsonConvert.DeserializeObject<Root>(sjson, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore

                });

                if (data.globalObjects.tweets != null)
                {
                    if (data.globalObjects.tweets.Count == 0)
                    {
                        //Debug.Print(">>" + sjson);
                        return "";
                    }
                }

                foreach (var item in data.globalObjects.tweets)
                {
                    if (data.globalObjects.users.ContainsKey(item.Value.user_id_str))
                    {

                        File.AppendAllText(pathFile, "https://twitter.com/" + data.globalObjects.users[item.Value.user_id_str].screen_name.ToString() + "/status/" + item.Key.ToString() + "?s=20\n");
                        //Debug.Print(data.globalObjects.users[item.Value.user_id_str].screen_name.ToString() + "/status/" + item.Key.ToString() + "?s=20");
                    }
                }
                info._COUNT_LINK += data.globalObjects.tweets.Count;
                cursor = Regex.Match(sjson, "cursor\":{\"value\":\"refresh:(.*?)\",\"cursorType\":\"Top\"").Groups[1].Value;
                return cursor;
            }
            catch
            {
                return null;
            }

        }
    }
    public class Root
    {
        public GlobalObjects globalObjects { get; set; }
        public Timeline timeline { get; set; }
    }
    public class GlobalObjects
    {
        public Dictionary<string, Tweet> tweets { get; set; }
        public Dictionary<string, User> users { get; set; }
        public Moments moments { get; set; }
        public Cards cards { get; set; }
        public Places places { get; set; }
        public Broadcasts broadcasts { get; set; }
        public Topics topics { get; set; }
        public Lists lists { get; set; }
    }
    public class Moments
    {
    }

    public class Cards
    {
    }

    public class Places
    {
    }

    public class Broadcasts
    {
    }

    public class Topics
    {
    }

    public class Lists
    {
    }
    public class TimelinesDetails
    {
        public string controllerData { get; set; }
    }

    public class Details
    {
        public TimelinesDetails timelinesDetails { get; set; }
    }

    public class ClientEventInfo
    {
        public string component { get; set; }
        public string element { get; set; }
        public Details details { get; set; }
        public string action { get; set; }
    }

    public class DisplayContext
    {
        public string reason { get; set; }
    }

    public class FeedbackInfo
    {
        public List<string> feedbackKeys { get; set; }
        public DisplayContext displayContext { get; set; }
        public ClientEventInfo clientEventInfo { get; set; }
    }

    public class Item
    {
        public Content content { get; set; }
        public ClientEventInfo clientEventInfo { get; set; }
        public FeedbackInfo feedbackInfo { get; set; }
    }

    public class Cursor
    {
        public string value { get; set; }
        public string cursorType { get; set; }
    }

    public class Operation
    {
        public Cursor cursor { get; set; }
    }

    public class Entry
    {
        public string entryId { get; set; }
        public string sortIndex { get; set; }
        public ContentEntry content { get; set; }
    }
    public class Tweet
    {
        public string created_at { get; set; }
        public long id { get; set; }
        public string id_str { get; set; }
        public string full_text { get; set; }
        public bool truncated { get; set; }
        public List<int> display_text_range { get; set; }
        public Entities entities { get; set; }
        public string source { get; set; }
        public object in_reply_to_status_id { get; set; }
        public object in_reply_to_status_id_str { get; set; }
        public long in_reply_to_user_id { get; set; }
        public string in_reply_to_user_id_str { get; set; }
        public string in_reply_to_screen_name { get; set; }
        public long user_id { get; set; }
        public string user_id_str { get; set; }
        public object geo { get; set; }
        public object coordinates { get; set; }
        public object place { get; set; }
        public object contributors { get; set; }
        public bool is_quote_status { get; set; }
        public long quoted_status_id { get; set; }
        public string quoted_status_id_str { get; set; }
        public int retweet_count { get; set; }
        public int favorite_count { get; set; }
        public int reply_count { get; set; }
        public int quote_count { get; set; }
        public long conversation_id { get; set; }
        public string conversation_id_str { get; set; }
        public bool favorited { get; set; }
        public bool retweeted { get; set; }
        public string lang { get; set; }
        public object supplemental_language { get; set; }
        public Ext ext { get; set; }
    }
    public class Content
    {
        public Tweet tweet { get; set; }
        public Item item { get; set; }
        public Operation operation { get; set; }
    }
    public class ContentEntry
    {
        public Operation operation { get; set; }
    }
    public class User
    {
        public long id { get; set; }
        public string id_str { get; set; }
        public string name { get; set; }
        public string screen_name { get; set; }
        public string location { get; set; }
        public string description { get; set; }
        public object url { get; set; }
        public Entities entities { get; set; }
        public bool @protected { get; set; }
        public int followers_count { get; set; }
        public int fast_followers_count { get; set; }
        public int normal_followers_count { get; set; }
        public int friends_count { get; set; }
        public int listed_count { get; set; }
        public string created_at { get; set; }
        public int favourites_count { get; set; }
        public object utc_offset { get; set; }
        public object time_zone { get; set; }
        public bool geo_enabled { get; set; }
        public bool verified { get; set; }
        public int statuses_count { get; set; }
        public int media_count { get; set; }
        public object lang { get; set; }
        public bool contributors_enabled { get; set; }
        public bool is_translator { get; set; }
        public bool is_translation_enabled { get; set; }
        public string profile_background_color { get; set; }
        public object profile_background_image_url { get; set; }
        public object profile_background_image_url_https { get; set; }
        public bool profile_background_tile { get; set; }
        public string profile_image_url { get; set; }
        public string profile_image_url_https { get; set; }
        public object profile_image_extensions_sensitive_media_warning { get; set; }
        public object profile_image_extensions_media_availability { get; set; }
        public object profile_image_extensions_alt_text { get; set; }
        public ProfileImageExtensionsMediaColor profile_image_extensions_media_color { get; set; }
        public ProfileImageExtensions profile_image_extensions { get; set; }
        public string profile_link_color { get; set; }
        public string profile_sidebar_border_color { get; set; }
        public string profile_sidebar_fill_color { get; set; }
        public string profile_text_color { get; set; }
        public bool profile_use_background_image { get; set; }
        public bool has_extended_profile { get; set; }
        public bool default_profile { get; set; }
        public bool default_profile_image { get; set; }
        public List<object> pinned_tweet_ids { get; set; }
        public List<object> pinned_tweet_ids_str { get; set; }
        public bool has_custom_timelines { get; set; }
        public bool can_dm { get; set; }
        public bool can_media_tag { get; set; }
        public bool following { get; set; }
        public bool follow_request_sent { get; set; }
        public bool notifications { get; set; }
        public bool muting { get; set; }
        public bool blocking { get; set; }
        public bool blocked_by { get; set; }
        public bool want_retweets { get; set; }
        public string advertiser_account_type { get; set; }
        public List<object> advertiser_account_service_levels { get; set; }
        public string profile_interstitial_type { get; set; }
        public string business_profile_state { get; set; }
        public string translator_type { get; set; }
        public List<object> withheld_in_countries { get; set; }
        public bool followed_by { get; set; }
        public Ext ext { get; set; }
        public bool require_some_consent { get; set; }
    }
    public class ProfileImageExtensionsMediaColor
    {
        public List<Palette> palette { get; set; }
    }
    public class Palette
    {
        public Rgb rgb { get; set; }
        public double percentage { get; set; }
    }
    public class Rgb
    {
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }
    }

    public class ProfileImageExtensions
    {
        public MediaStats mediaStats { get; set; }
    }
    public class Entities
    {
        public List<object> hashtags { get; set; }
        public List<object> symbols { get; set; }
        public List<UserMention> user_mentions { get; set; }
        public List<object> urls { get; set; }
        public List<Medium> media { get; set; }
        public Description description { get; set; }
        public Url url { get; set; }
    }
    public class Medium
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
        public List<object> faces { get; set; }
    }
    public class Description
    {
        public List<object> urls { get; set; }
    }
    public class UserMention
    {
        public string screen_name { get; set; }
        public string name { get; set; }
        public object id { get; set; }
        public string id_str { get; set; }
        public List<int> indices { get; set; }
    }
    public class Url
    {
        public string url { get; set; }
        public string expanded_url { get; set; }
        public string display_url { get; set; }
        public List<int> indices { get; set; }
    }
    public class R
    {
        public Ok ok { get; set; }
        public object missing { get; set; }
    }
    public class Ok
    {
        public bool superFollowEligible { get; set; }
        public bool superFollowing { get; set; }
        public bool superFollowedBy { get; set; }
    }

    public class SuperFollowMetadata
    {
        public R r { get; set; }
        public int ttl { get; set; }
    }

    public class Ext
    {
        public SuperFollowMetadata superFollowMetadata { get; set; }
        public MediaStats mediaStats { get; set; }
        public HighlightedLabel highlightedLabel { get; set; }
    }
    public class HighlightedLabel
    {
        public R r { get; set; }
        public int ttl { get; set; }
    }
    public class MediaStats
    {
        public R r { get; set; }
        public int ttl { get; set; }
    }

    public class AddEntries
    {
        public List<Entry> entries { get; set; }
    }

    public class Instruction
    {
        public AddEntries addEntries { get; set; }
    }

    public class Givefeedback
    {
        public string feedbackType { get; set; }
        public string prompt { get; set; }
        public string confirmation { get; set; }
        public List<string> childKeys { get; set; }
        public bool hasUndoAction { get; set; }
        public string confirmationDisplayType { get; set; }
        public ClientEventInfo clientEventInfo { get; set; }
        public string icon { get; set; }
    }

    public class Notrelevant
    {
        public string feedbackType { get; set; }
        public string prompt { get; set; }
        public string confirmation { get; set; }
        public bool hasUndoAction { get; set; }
        public string confirmationDisplayType { get; set; }
        public ClientEventInfo clientEventInfo { get; set; }
    }

    public class Notcredible
    {
        public string feedbackType { get; set; }
        public string prompt { get; set; }
        public string confirmation { get; set; }
        public bool hasUndoAction { get; set; }
        public string confirmationDisplayType { get; set; }
        public ClientEventInfo clientEventInfo { get; set; }
    }

    public class FeedbackActions
    {
        public Givefeedback givefeedback { get; set; }
        public Notrelevant notrelevant { get; set; }
        public Notcredible notcredible { get; set; }
    }

    public class ResponseObjects
    {
        public FeedbackActions feedbackActions { get; set; }
    }

    public class Timeline
    {
        public string id { get; set; }
        public List<Instruction> instructions { get; set; }
        public ResponseObjects responseObjects { get; set; }
    }
}
