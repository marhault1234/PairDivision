﻿using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1
{
    public static class Logic
    {
        // 1コート用組み合わせ表
        private static readonly List<string> C1_4P = new List<string>() { "ab-cd", "ac-bd", "ad-bc" };
        private static readonly List<string> C1_5P = new List<string>() { "ab-cd", "ac-be", "ae-db", "ad-ce", "bc-de" };
        private static readonly List<string> C1_6P = new List<string>() { "ab-cd", "ae-bf", "ce-df", "ac-be", "af-db", "ac-de", "bc-df", "ae-cf", "bd-ef", "ad-be", "ab-cf", "bc-de", "af-dc", "bf-ec", "ad-ef" };
        private static readonly List<string> C1_7P = new List<string>() { "ab-cd", "ae-fg", "bc-de", "af-bg", "ce-df", "ab-cg", "dg-fe", "ac-be", "ad-fg", "bd-cf", "ag-eb", "cd-eg", "ac-bf", "ad-ef", "bc-dg", "bf-eg", "ac-de", "af-cg", "bd-ef", "ad-bg", "ce-fg", "ae-db", "bg-fc", "ac-df", "ag-ec", "be-dg", "ab-df", "ae-cf", "bc-eg", "bf-dg", "ag-dc", "af-eb", "cg-fd", "bc-ef", "ag-ed" };
        private static readonly List<string> C1_8P = new List<string>() { "ab-cd", "ef-gh", "ac-be", "dg-fh", "ae-db", "ch-gf", "ad-ce", "bf-gh", "bc-de", "ag-fh", "ab-cf", "dh-ge", "af-db", "cg-eh", "ac-df", "bg-eh", "bc-df", "ah-ge", "ae-bf", "cg-dh", "af-ec", "bh-gd", "be-cf", "ad-gh", "ad-ef", "bg-ch", "bf-ed", "ag-ch", "cd-ef", "ag-bh", "ab-cg", "de-fh", "ab-dg", "cf-eh", "ac-dg", "be-fh", "bg-dc", "ah-fe", "ae-bg", "cf-dh", "ac-eg", "bh-fd", "bc-eg", "af-dh", "ag-ed", "bf-ch", "bd-eg", "ah-fc", "ce-dg", "af-bh", "ab-fg", "cd-eh", "ac-fg", "bd-eh", "bc-fg", "ae-dh", "ad-fg", "be-ch", "bg-fd", "ah-ec", "cg-fd", "ae-bh", "ag-fe", "bc-dh", "be-fg", "ah-dc", "cg-fe", "ad-bh", "de-fg", "ab-ch" };
        private static readonly List<string> C1_9P = new List<string>() { "ab-cd", "ef-gh", "ac-bi", "de-fg", "ai-hb", "ce-df", "ag-hi", "bc-de", "fh-gi", "ac-be", "dg-fh", "ab-di", "cf-eg", "ah-ci", "bd-ef", "bg-hi", "ad-ce", "af-gh", "bc-di", "eg-fi", "ab-ch", "df-eh", "ag-bi", "cg-ed", "ah-fi", "bf-dc", "eh-gi", "ae-db", "ch-gf", "ad-ci", "be-fg", "bh-ci", "ae-df", "cg-hi", "ad-bf", "ae-gh", "bc-ei", "dg-fi", "ab-dh", "cf-eh", "ai-gc", "bd-eg", "bi-hf", "af-dc", "bg-eh", "ac-ei", "dh-fi", "ac-bg", "dh-ge", "ai-fb", "cf-dg", "ah-ei", "bc-ef", "dh-gi", "af-cb", "de-gi", "ad-ch", "be-fh", "bg-ci", "ae-dg", "cf-hi", "af-eb", "ag-dh", "bd-ei", "cg-fi", "bh-dc", "ag-fe", "ah-di", "bg-ec", "ef-hi", "ab-dg", "af-ch", "ce-di", "bf-gh", "ae-bi", "ch-fd", "ad-gi", "bc-fg", "bh-ei", "ac-ef", "bd-gh", "ai-ed", "ag-fc", "bh-di", "cg-eh", "af-ci", "bd-fg", "ce-hi", "ab-eg", "ah-fd", "bf-ci", "dh-ei", "ag-dc", "be-fi", "ah-gb", "cd-eh", "ai-gf", "bc-dg", "ae-fh", "bi-fd", "ce-gi", "ae-bh", "cd-gh", "ad-fi", "be-ch", "bf-gi", "ac-eg", "bd-fh", "cf-di", "ai-ge", "ac-eh", "bi-gd", "ab-fh", "cf-ei", "ad-fg", "bh-ed", "ah-gc", "di-fe", "bg-ch", "af-ei", "cg-di", "bf-ch", "ae-dh", "be-gi", "ag-fb", "cd-hi" };
        private static readonly List<string> C1_10P = new List<string>() { "ab-cd", "ef-gh", "ai-bj", "ce-df", "gi-hj", "ac-be", "dg-fh", "aj-ic", "bf-ed", "ag-hi", "bc-dj", "eg-fi", "ah-bj", "cg-ed", "fh-ij", "ae-db", "ch-gf", "bi-cj", "ad-ef", "bg-hi", "ac-dj", "eh-fi", "ab-gj", "cf-dg", "ei-hj", "ad-ce", "bh-gf", "aj-id", "be-cf", "ch-gi", "aj-db", "ei-hg", "ab-fj", "cf-eg", "dh-ij", "bc-de", "af-gh", "bi-dj", "af-ec", "dh-gi", "ab-cj", "fg-hi", "ae-bj", "df-eg", "ci-hj", "ac-bf", "dg-eh", "cd-ij", "ae-bf", "ah-gj", "bc-di", "ej-gf", "ai-hb", "cd-eh", "fi-gj", "af-db", "cg-eh", "ai-ej", "bc-df", "bg-hj", "ad-ci", "ej-hf", "ag-bi", "cf-dh", "ei-gj", "ac-df", "be-gh", "be-ij", "ad-cg", "ah-fi", "be-cj", "di-gf", "aj-hc", "bg-ed", "bf-hi", "ae-cj", "di-hf", "ag-cj", "bf-dg", "ah-ei", "bd-ej", "cf-gi", "bj-hc", "ad-eg", "ci-hf", "ae-dj", "bg-fi", "aj-hd", "bg-ec", "ah-fj", "bi-ec", "dg-fj", "ai-hc", "bg-fe", "ai-hd", "cd-ej", "bf-gj", "bh-ci", "ag-fd", "bh-ei", "ac-fj", "de-gi", "bd-hj", "ae-cg", "bh-fj", "ac-ei", "dj-hf", "ag-ci", "bd-eh", "af-gi", "bc-fj", "de-hi", "bj-gc", "af-eg", "bh-di", "af-dj", "ce-gi", "cd-hj", "ab-eg", "cf-hj", "ad-bi", "ej-hg", "ab-fi", "ch-fe", "dg-ij", "ag-cb", "dh-fe", "ce-ij", "ab-dg", "fh-gj", "ac-bi", "di-fe", "aj-he", "bd-cg", "ai-fj", "bh-dc", "ef-ij", "ah-gb", "cd-gh", "ae-fi", "bj-fd", "ce-hi", "ad-gj", "bc-fg", "be-hj", "ai-ed", "cj-gf", "ch-di", "ag-fb", "cj-he", "ab-ei", "dj-hg", "af-ci", "bf-dh", "ai-ge", "cf-dj", "be-gi", "dh-ej", "af-cg", "bj-ih", "ah-cb", "df-ej", "bi-gc", "ad-eh", "aj-gf", "bd-ei", "ch-gj", "bi-fc", "ah-fd", "aj-ge", "ci-ed", "bj-if", "ac-gh", "be-fh", "ag-di", "be-fj", "cd-gi", "ah-ij", "bh-ec", "bg-dj", "ac-fh", "dg-ej", "ad-fi", "bf-ch", "bj-ge", "cf-di", "ae-gh", "de-ij", "af-bh", "ci-gj", "ab-dh", "ci-fe", "cj-gd", "ab-eh", "cj-if", "ag-dh", "bi-fe", "cg-ej", "bd-gh", "ae-fj" };
        private static readonly List<string> C1_11P = new List<string>() { "ab-cd", "ef-gh", "ai-jk", "bc-de", "fg-hi", "aj-bk", "ce-df", "gi-hj", "ab-ck", "df-eg", "hk-ji", "ac-be", "dg-fh", "bj-ik", "ad-ce", "fh-gj", "ak-ib", "cg-ed", "fi-hj", "ak-db", "cf-eg", "ah-ij", "bc-dk", "ei-gf", "ah-jk", "bf-dc", "eh-gi", "aj-ck", "bd-ef", "gk-ih", "ab-cj", "dh-fe", "gj-ik", "ad-be", "cf-gh", "ci-jk", "af-db", "ej-hg", "ac-ik", "bg-ed", "fi-hk", "ab-dj", "cf-eh", "ag-ij", "bc-ek", "di-gf", "bh-jk", "af-dc", "ei-gj", "ah-bk", "cg-fd", "ej-ih", "ac-dk", "bf-eg", "bi-hj", "ae-ck", "dh-fi", "aj-gk", "bf-ec", "dg-hi", "bk-jc", "ae-df", "gj-hk", "ai-cb", "dg-eh", "fj-ik", "ac-bf", "di-ge", "ch-jk", "af-eb", "dj-hg", "bk-ic", "ag-ed", "fk-jh", "ad-bi", "cg-eh", "ai-fj", "bd-ek", "cf-gi", "dj-hk", "ae-cf", "bh-gi", "aj-dk", "bg-ec", "ai-hf", "bj-dk", "ci-fe", "ag-hj", "cd-ek", "bg-fh", "di-jk", "ab-cg", "ei-hf", "cj-dk", "ae-bg", "bh-fi", "ak-je", "ch-fd", "bj-ig", "ad-ek", "ch-fi", "bj-gk", "ac-dg", "eh-fj", "ak-id", "bc-fg", "ei-hk", "ad-cj", "bh-fe", "cg-ij", "ak-eb", "dg-fj", "ah-ik", "bg-dc", "ei-fj", "ah-ck", "bd-fg", "ej-hk", "ad-ci", "be-gh", "bf-ij", "ac-fk", "dh-ei", "cj-gk", "ag-db", "eh-fk", "aj-ib", "ce-dh", "fi-gj", "af-bk", "ch-gd", "ej-ik", "ae-cg", "bf-dh", "ai-cj", "bd-fk", "ek-hg", "bj-ic", "ag-fd", "ae-hi", "bj-ek", "cf-di", "bh-gj", "af-dk", "ci-ge", "ab-hj", "cd-fk", "ei-gk", "aj-hc", "bh-ed", "fk-ig", "bc-dj", "af-eg", "ch-ij", "ak-fe", "bg-dh", "ai-dj", "bf-ck", "ej-gk", "ah-bi", "ce-di", "fg-hk", "aj-eb", "cd-gi", "ah-fj", "bk-fe", "cd-hi", "dj-gk", "ab-fg", "ce-hi", "ck-je", "ad-bh", "fk-jg", "bc-di", "ae-fh", "dg-ij", "cf-ek", "ag-bh", "bi-dj", "ag-ck", "ef-ik", "bj-hc", "ah-ed", "af-gi", "de-jk", "bc-fh", "ai-gk", "be-cj", "df-hj", "bg-ik", "ac-fg", "dh-ej", "bi-dk", "ac-eh", "bg-fi", "ak-jf", "ci-hg", "ae-dj", "de-fk", "bh-gc" };
        private static readonly List<string> C1_12P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "ac-be", "dg-fh", "ai-jk", "bc-dl", "eg-fi", "hk-jl", "ae-db", "ch-gf", "bj-ik", "ad-cl", "ef-hi", "gj-kl", "ac-de", "bf-gh", "ci-jk", "ad-bl", "eh-gi", "fk-jl", "bd-ce", "ag-fh", "dj-ik", "al-cb", "fg-hi", "ej-kl", "ab-cf", "dh-ge", "ek-ji", "af-bl", "cg-dh", "fj-ik", "ab-el", "cg-di", "ak-jh", "be-cl", "df-gi", "bk-jh", "al-ec", "df-hi", "aj-gk", "bd-el", "cf-gi", "cj-hk", "ae-dl", "bg-fi", "dk-jh", "ac-fl", "bg-eh", "gk-ji", "cd-el", "af-bh", "hi-jk", "bc-fl", "ag-ed", "aj-il", "bd-ck", "ef-gj", "hk-il", "ad-bf", "ch-ge", "bi-jl", "ak-dc", "eh-fj", "gl-ki", "ac-df", "bg-ei", "ej-hk", "af-dl", "bh-gc", "cj-il", "ab-dk", "ej-hg", "fk-il", "bf-dc", "ah-ge", "di-jl", "ab-ck", "fh-gj", "ei-kl", "ae-bf", "ci-hd", "bj-gk", "cf-dl", "ag-ei", "fk-jh", "ag-bl", "ch-ed", "el-ji", "af-bk", "ci-hg", "dj-kl", "ae-cf", "bh-gd", "fi-jl", "ak-eb", "dg-hi", "cl-kj", "bc-ef", "ah-gd", "gl-ji", "ac-ek", "bh-fd", "hl-ji", "bk-ec", "ad-fg", "ai-kl", "bc-dj", "ei-fj", "gk-hl", "af-ed", "bi-gc", "gj-hk", "bd-fl", "ah-ec", "bk-il", "ad-cj", "ej-ig", "fk-hl", "be-df", "ag-ch", "ck-il", "ad-bj", "fi-gj", "ek-hl", "cd-ef", "ah-gb", "di-kl", "aj-cb", "ei-hj", "fk-gl", "ab-cg", "de-fh", "al-kj", "bi-dc", "fj-ih", "ek-gl", "ab-dg", "cf-eh", "bl-kj", "ac-di", "gi-hj", "ek-fl", "ac-dg", "be-fh", "ai-bj", "cl-kd", "ek-gf", "ai-hj", "bf-el", "cg-dj", "ah-ik", "ce-fl", "bg-di", "aj-hl", "be-dk", "ch-fi", "cj-gk", "al-fe", "bi-hd", "dj-gk", "ag-cl", "bf-ei", "bh-jl", "ae-dk", "cj-gf", "bi-hk", "de-fl", "ag-ci", "cj-hl", "ak-fd", "bi-he", "eg-jk", "bc-gl", "af-dh", "aj-ic", "bk-dl", "eh-fk", "ai-gj", "bd-gl", "ce-fi", "dh-jl", "af-ck", "bh-gi", "ak-je", "cg-dl", "bi-hf", "bj-ek", "al-gd", "ch-ei", "ak-jf", "bl-ge", "cf-dh", "bj-ic", "al-kd", "ek-hg", "ai-fj", "cl-ge", "bf-di" };
        private static readonly List<string> C1_13P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "ac-bm", "de-fg", "hi-jk", "am-lb", "ce-df", "gi-hj", "ak-lm", "bc-de", "fh-gi", "jl-km", "ac-be", "dg-fh", "ik-jm", "ab-cl", "df-eh", "gj-ik", "al-cm", "bd-ef", "gk-ih", "aj-lm", "bd-cf", "ei-hg", "aj-kl", "bc-dm", "eg-fi", "hk-jl", "ad-bm", "ce-fg", "hj-il", "am-kb", "cd-eg", "fj-ih", "bl-km", "ae-dc", "fh-gj", "im-lk", "ad-be", "ch-gf", "il-jm", "ab-ck", "dg-eh", "fk-ji", "bl-cm", "af-ed", "gh-jk", "ai-lm", "bf-ec", "dh-gi", "bj-kl", "ac-dm", "eh-fi", "gl-kj", "ab-em", "cg-fd", "hl-ki", "am-jb", "ce-dh", "fi-gj", "cl-km", "ad-bf", "ej-hg", "ak-il", "bc-em", "di-gf", "hk-jm", "al-db", "ch-fe", "gl-ji", "ak-cm", "bg-ed", "fk-ih", "bm-lj", "ad-cf", "ei-gj", "hl-km", "af-cb", "di-ge", "hm-lj", "ab-dk", "cg-eh", "fl-ji", "bk-cm", "ae-dg", "fj-hk", "bi-lm", "ae-cf", "dj-hg", "bi-kl", "ac-em", "dh-fi", "gm-kj", "ac-dl", "bg-fe", "hj-im", "al-kb", "ch-fd", "ei-gk", "cj-lm", "af-eb", "dj-ig", "ah-kl", "be-dm", "ci-gf", "ah-jk", "bm-ld", "ci-fe", "gl-jh", "am-kd", "be-cg", "fl-ih", "aj-km", "bg-dc", "ej-hf", "ck-il", "ae-dm", "bh-gf", "ai-jk", "cl-dm", "bf-eh", "gk-il", "am-jc", "bf-dg", "ei-hj", "dk-lm", "ag-cb", "ei-fj", "bk-hl", "cd-em", "ag-fh", "bj-ik", "am-ld", "ci-ge", "fj-hl", "bm-kd", "ag-ec", "fl-kh", "ai-jm", "bg-fc", "di-he", "ck-jl", "ab-fm", "dg-ej", "hm-ki", "bd-cl", "af-eg", "hl-im", "ak-jb", "cg-dh", "ek-if", "dl-jm", "ae-bg", "ch-fi", "dj-kl", "ac-fm", "bh-ge", "cj-ik", "am-le", "bh-fd", "gm-ji", "al-kc", "bh-ed", "fk-ig", "el-jm", "ag-db", "ch-fj", "dk-il", "bc-fm", "ah-ge", "di-jk", "bl-em", "af-cg", "ah-ij", "ek-lm", "bh-dc", "fk-jg", "cl-im", "ag-fb", "dh-ej", "ek-il", "ad-fm", "bg-ch", "ek-ji", "al-fm", "bd-gh", "ci-jl", "ck-dm", "ah-eb", "fl-ig", "bj-km", "ad-cg", "ef-hk", "ai-jl", "bm-fd", "ce-hi", "gl-jm", "ak-dc", "bi-fe" };
        private static readonly List<string> C1_14P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "am-bn", "ce-df", "gi-hj", "km-ln", "ac-be", "dg-fh", "ik-jm", "an-lb", "cg-ed", "fj-ih", "ak-lm", "bc-dn", "eg-fi", "hk-jl", "ac-mn", "bd-ef", "gk-ih", "jn-ml", "ad-be", "ch-gf", "il-jm", "ab-kn", "cf-dg", "eh-ij", "bm-lk", "ad-cn", "ei-hf", "gl-kj", "bn-mc", "ae-df", "gj-hk", "im-ln", "ae-dc", "bf-gh", "il-km", "an-jb", "ce-fg", "di-hj", "ck-lm", "ab-dn", "eh-gi", "fk-jl", "an-md", "bc-ef", "gj-ik", "hl-mn", "bc-de", "af-gh", "jk-lm", "ai-bn", "df-eg", "ci-hj", "dl-km", "ab-cn", "fg-hi", "ej-kl", "bd-mn", "ae-cf", "hk-ji", "gm-ln", "ac-bf", "dh-ge", "in-kj", "al-bm", "ce-dh", "fi-gj", "el-km", "ab-fn", "cg-dh", "il-jn", "am-kb", "cd-fh", "ej-ig", "fm-lk", "ab-en", "cg-di", "hm-kj", "al-cn", "bg-ed", "fk-ih", "aj-lm", "bc-en", "dg-fi", "hl-jm", "ac-kn", "bg-fd", "eh-ik", "bl-jm", "ac-en", "di-hf", "gk-jm", "bn-lc", "ag-ed", "fj-hk", "al-im", "be-dn", "ci-gf", "hm-lk", "an-jc", "bf-eg", "dk-ih", "cm-lj", "ad-en", "bi-gf", "hn-kj", "am-lc", "bh-ed", "fi-gk", "dm-lj", "af-cn", "bg-eh", "il-kn", "aj-bm", "ch-fe", "dj-ig", "gl-km", "ce-dn", "af-bh", "jl-kn", "ai-bm", "dh-fe", "ci-gj", "ak-ln", "bd-cm", "eg-fj", "hl-ki", "cd-mn", "ae-bf", "gh-il", "jn-mk", "af-db", "cg-eh", "im-jn", "ak-bl", "cd-ei", "fj-hg", "bk-ln", "ad-cm", "ej-hf", "gl-ki", "an-me", "bd-cf", "gl-jh", "im-kn", "ad-cf", "bg-ei", "hn-lj", "am-kc", "bh-fd", "ek-ig", "el-jm", "bc-fn", "ah-gd", "ak-ji", "bl-mn", "cf-di", "ej-hg", "ck-ln", "ab-dm", "ei-fj", "gk-hl", "bn-me", "ag-dc", "fk-ji", "al-hm", "bd-fn", "cg-ei", "hn-lk", "am-jc", "be-fh", "dk-ig", "fl-jm", "ab-gn", "ch-di", "em-kj", "an-ld", "bg-ec", "fl-ih", "aj-km", "cd-fn", "bi-he", "gm-lj", "bk-cn", "ag-fd", "ek-jh", "bi-lm", "af-dn", "ch-ei", "gn-kj", "bm-lc", "ag-fe", "dj-hk", "cl-im", "ae-fn", "bh-gd", "bj-ik", "al-mn" };
        private static readonly List<string> C1_15P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "am-no", "bc-de", "fg-hi", "jk-lm", "an-bo", "ce-df", "gi-hj", "km-ln", "ab-co", "df-eg", "hj-ik", "lo-nm", "ac-be", "dg-fh", "ik-jm", "al-no", "bd-cf", "eh-gi", "jl-kn", "ao-mb", "cg-ed", "fj-ih", "kl-mo", "ac-bn", "dh-fe", "gk-ji", "al-mn", "bc-do", "eg-fi", "hk-jl", "bn-mo", "ad-ce", "fh-gj", "il-km", "ao-nc", "bf-ed", "gh-ik", "jn-ml", "ad-bo", "ce-fg", "hl-ji", "ko-nm", "ae-db", "ch-gf", "il-jm", "ak-no", "be-cf", "di-hg", "jm-kn", "ao-lb", "cf-dg", "ei-hj", "ko-nl", "ab-cm", "dg-eh", "fi-jk", "bm-ln", "ac-do", "ei-hf", "gl-kj", "cn-mo", "af-db", "eh-gj", "in-lk", "am-co", "bg-ed", "fk-ih", "jo-ml", "ab-dn", "ch-fe", "gi-jl", "an-mk", "bc-eo", "di-gf", "hm-kj", "bn-lo", "af-dc", "ej-ig", "hk-lm", "bo-nc", "ae-df", "gj-hk", "im-ln", "ab-eo", "cf-dh", "gk-il", "jn-mo", "ac-bf", "di-ge", "hl-jm", "bk-no", "af-ec", "dj-hg", "im-kn", "ao-lc", "bg-fd", "ek-ih", "jn-lo", "ad-bm", "cg-eh", "fl-ji", "bk-mn", "ae-co", "dh-fi", "gm-kj", "cl-no", "ae-bf", "dj-ig", "hn-lk", "bo-mc", "ag-ed", "fj-hk", "io-ml", "an-dc", "bg-fe", "hl-ki", "aj-mn", "be-do", "ci-gf", "hj-kn", "am-lo", "bd-cg", "ej-hf", "il-ko", "am-bn", "cd-eh", "fi-gj", "al-km", "bo-nd", "ci-fe", "gl-jh", "ck-mn", "ad-eo", "bh-gf", "in-kj", "bl-mo", "ag-dc", "ei-fj", "hm-kn", "bl-co", "af-dg", "ej-hk", "io-nl", "ac-dm", "bh-fe", "gm-ji", "ak-ln", "ce-do", "bi-gf", "hn-lj", "ak-mo", "bc-eg", "dh-fj", "im-ko", "an-lb", "ch-gd", "ei-fk", "aj-lm", "co-nd", "bh-ge", "fl-ki", "bj-mn", "ac-fo", "dh-ei", "gl-jm", "ck-no", "ag-db", "ek-hf", "in-lj", "ao-md", "bf-cg", "ek-ji", "hm-ln", "ab-fo", "ci-ed", "gk-hl", "cm-jn", "ad-fo", "bi-ge", "hm-jn", "ak-lo", "bd-ch", "ef-gj", "in-ko", "al-bm", "cf-di", "ek-hg", "bj-lm", "an-do", "cg-ei", "fl-jh", "dk-mn", "bc-fo", "ah-ge", "im-jn", "bk-lo", "ag-ec" };
        private static readonly List<string> C1_16P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "mn-op", "ac-be", "dg-fh", "ik-jm", "lo-np", "ae-db", "ch-gf", "im-lj", "kp-on", "ad-ce", "bf-gh", "il-km", "jn-op", "bc-de", "ag-fh", "jk-lm", "io-np", "ab-cf", "dh-ge", "ij-kn", "lp-om", "af-db", "cg-eh", "in-lj", "ko-mp", "ac-df", "bg-eh", "ik-ln", "jo-mp", "bc-df", "ah-ge", "jk-ln", "ip-om", "ae-bf", "cg-dh", "im-jn", "ko-lp", "af-ec", "bh-gd", "in-mk", "jp-ol", "be-cf", "ad-gh", "jm-kn", "il-op", "ad-ef", "bg-ch", "il-mn", "jo-kp", "bf-ed", "ag-ch", "jn-ml", "io-kp", "cd-ef", "ag-bh", "kl-mn", "io-jp", "ab-cg", "de-fh", "ij-ko", "lm-np", "ab-dg", "cf-eh", "ij-lo", "kn-mp", "ac-dg", "be-fh", "ik-lo", "jm-np", "bg-dc", "ah-fe", "jo-lk", "ip-nm", "ae-bg", "cf-dh", "im-jo", "kn-lp", "ac-eg", "bh-fd", "ik-mo", "jp-nl", "bc-eg", "af-dh", "jk-mo", "in-lp", "ag-ed", "bf-ch", "io-ml", "jn-kp", "bd-eg", "ah-fc", "jl-mo", "ip-nk", "ce-dg", "af-bh", "km-lo", "in-jp", "ab-fg", "cd-eh", "ij-no", "kl-mp", "ac-fg", "bd-eh", "ik-no", "jl-mp", "bc-fg", "ae-dh", "jk-no", "im-lp", "ad-fg", "be-ch", "il-no", "jm-kp", "bg-fd", "ah-ec", "jo-nl", "ip-mk", "cg-fd", "ae-bh", "ko-nl", "im-jp", "ag-fe", "bc-dh", "io-nm", "jk-lp", "be-fg", "ah-dc", "jm-no", "ip-lk", "cg-fe", "ad-bh", "ko-nm", "il-jp", "de-fg", "ab-ch", "lm-no", "ij-kp", "ac-bi", "de-fj", "gk-hl", "an-mo", "bd-cp", "eg-fi", "hk-jl", "bn-mo", "ap-dc", "ei-hf", "gj-kl", "co-nm", "ad-bp", "ei-hg", "fk-jl", "dm-no", "ap-cb", "fh-gi", "el-kj", "en-mo", "af-bp", "cg-di", "hj-km", "ao-nl", "bc-ep", "df-gi", "hj-lm", "an-ko", "be-dp", "cf-gi", "hl-km", "ao-nj", "ce-dp", "bg-fi", "hk-jn", "am-lo", "bf-cp", "dg-ei", "hn-lj", "am-ko", "bd-fp", "ci-ge", "hl-kn", "am-jo", "cd-fp", "bi-ge", "hm-jn", "ak-lo", "bf-ep", "cd-hi", "gk-jm", "bo-nl", "ae-cp", "df-hi", "gl-jm", "bo-nk", "ad-ep", "ci-hf", "gm-lk", "bn-jo" };
        private static readonly List<string> C1_17P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "mn-op", "ac-bq", "de-fg", "hi-jk", "lm-no", "aq-pb", "ce-df", "gi-hj", "km-ln", "ao-pq", "bc-de", "fh-gi", "jl-km", "np-oq", "ac-be", "dg-fh", "ik-jm", "lo-np", "ab-dq", "cf-eg", "hj-il", "kn-mo", "ap-cq", "bd-ef", "gk-ih", "jn-ml", "bo-pq", "ad-ce", "fj-hg", "im-lk", "an-op", "bc-dq", "eg-fi", "hk-jl", "mo-nq", "ab-cp", "df-eh", "gj-ik", "ln-mp", "ao-bq", "cg-ed", "fh-ij", "ko-ml", "ap-nq", "bf-dc", "ei-hg", "jk-ln", "mp-oq", "ae-db", "ch-gf", "il-jm", "kp-on", "ad-cq", "bg-fe", "hl-ki", "jo-nm", "bp-cq", "ae-df", "gj-hk", "in-ml", "co-pq", "af-db", "eh-gj", "il-kn", "am-op", "bc-eq", "dg-fi", "hm-kj", "lq-on", "ab-dp", "cf-eh", "gl-ji", "kp-nm", "aq-oc", "be-dg", "fk-ih", "jm-lo", "bn-pq", "af-dc", "ej-ig", "hl-km", "bo-np", "ac-eq", "dh-fi", "gk-jl", "mq-pn", "ac-bo", "dh-ge", "fj-ik", "lp-om", "an-bq", "cg-fd", "ei-hj", "kn-lo", "ap-mq", "bf-ec", "di-hg", "jn-mk", "lp-oq", "af-cb", "di-ge", "hm-lj", "ko-nq", "ad-cp", "bh-fe", "gi-kl", "jp-nm", "bq-oc", "ag-ed", "fj-hk", "im-lo", "cn-pq", "ae-bf", "dj-hg", "im-kn", "al-op", "be-dq", "ci-gf", "hn-kj", "lq-om", "bc-dp", "ag-fe", "hm-ji", "kp-nl", "aq-od", "bg-ec", "fl-ih", "jm-ko", "dn-pq", "ae-cf", "bh-gi", "jo-lk", "an-mp", "cd-eq", "bg-fh", "in-kj", "lp-mq", "ab-do", "cg-eh", "fl-ji", "ko-mp", "aq-nc", "bd-fg", "ek-ih", "jn-lo", "bp-mq", "ad-cg", "ei-hf", "jo-nk", "am-lp", "bc-fq", "dh-ei", "gm-kj", "lq-pn", "ac-do", "bh-ge", "fk-il", "jp-om", "bn-cq", "af-dg", "ej-hk", "io-nl", "cm-pq", "ag-db", "eh-fj", "io-lk", "bm-np", "ad-eq", "ch-fi", "gl-jm", "kp-nq", "bo-dc", "ah-fe", "gj-im", "ko-lp", "aq-nd", "bg-fc", "ej-ik", "hn-ml", "dp-oq", "ab-cg", "ej-if", "hl-kn", "bm-op", "ac-fq", "dj-ge", "hk-im", "al-no", "bp-dq", "ci-fe", "gl-jh", "kq-nm", "ao-bp", "ch-ed", "fg-ij", "kl-mp", "an-oq" };
        private static readonly List<string> C1_18P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "mn-op", "aq-br", "ce-df", "gi-hj", "km-ln", "oq-pr", "ac-be", "dg-fh", "ik-jm", "lo-np", "ar-qc", "bf-ed", "gk-ih", "jn-ml", "ao-pq", "bc-dr", "eg-fi", "hk-jl", "mo-nq", "ap-br", "cg-ed", "fj-ih", "ko-ml", "np-qr", "ae-db", "ch-gf", "il-jm", "kp-on", "bq-cr", "ad-ef", "gh-jk", "im-ln", "bo-pq", "ac-dr", "eh-fi", "gj-kl", "mp-nq", "ab-or", "cf-dg", "ei-hj", "kn-lo", "mq-pr", "ad-ce", "bg-fh", "il-km", "jp-on", "ar-qd", "be-cf", "gi-jk", "hl-mn", "cp-oq", "ar-db", "eg-hi", "fk-jl", "mq-po", "ab-nr", "ce-fg", "dh-ij", "kn-mo", "lp-qr", "bc-de", "af-gh", "jk-lm", "in-op", "bq-dr", "ae-cf", "hj-ik", "gl-mn", "do-pq", "ab-cr", "fh-gi", "ej-kl", "np-oq", "am-br", "df-eg", "ch-ij", "ln-mo", "kq-pr", "ac-bf", "dg-eh", "ik-jn", "lo-mp", "cd-qr", "af-eb", "gi-hl", "jm-kn", "eo-pq", "ab-fr", "cg-dh", "in-lj", "ko-mp", "aq-er", "bc-df", "gj-hl", "im-kn", "fp-oq", "ae-br", "cg-di", "hm-kj", "lq-on", "ap-cr", "bg-ed", "fi-hk", "jo-ml", "an-pq", "bc-er", "di-gf", "hm-lj", "ko-nq", "bp-cr", "ag-ed", "fj-hk", "io-ml", "bp-nq", "ac-er", "dh-fi", "gj-km", "lq-pn", "ar-oc", "bd-fg", "ek-ih", "jo-nl", "ap-mq", "bd-er", "ci-gf", "hm-lk", "jq-on", "ar-pd", "bg-ec", "fk-ji", "ho-ml", "cn-pq", "ad-er", "bi-gf", "hn-kj", "lq-om", "bp-dr", "ag-ec", "fl-ih", "jo-mk", "dp-nq", "ac-fr", "bh-ge", "il-kn", "jp-om", "br-qe", "af-dc", "gl-ji", "hn-mk", "gq-po", "cd-er", "ah-fb", "jn-lk", "io-mp", "cr-qe", "ad-bf", "hj-il", "gk-mn", "hp-oq", "bc-fr", "ad-ei", "gm-lj", "kq-pn", "bc-or", "ag-fd", "ej-hk", "in-lo", "bm-pq", "af-dr", "cg-eh", "im-jn", "ko-lp", "dq-er", "ac-bg", "fj-hl", "im-ko", "ep-nq", "bd-fr", "ag-ch", "ik-jo", "lp-nm", "ar-qf", "bg-dc", "ej-ik", "ho-nl", "cp-mq", "ae-fr", "bh-gd", "io-lj", "kp-nm", "br-qf", "ad-cg", "el-ih", "jk-no", "dp-mq", "be-fr", "ag-ci" };
        private static readonly List<string> C1_19P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "mn-op", "aq-rs", "bc-de", "fg-hi", "jk-lm", "no-pq", "ar-bs", "ce-df", "gi-hj", "km-ln", "oq-pr", "ab-cs", "df-eg", "hj-ik", "ln-mo", "ps-rq", "ac-be", "dg-fh", "ik-jm", "lo-np", "br-qs", "ad-ce", "fh-gj", "il-km", "np-or", "as-qb", "cg-ed", "fj-ih", "ko-ml", "nq-pr", "as-db", "cf-eg", "hl-ji", "kn-mo", "ap-qr", "bc-ds", "ei-gf", "hk-jl", "mq-on", "ap-rs", "bf-dc", "eh-gi", "jn-lk", "mp-oq", "ar-cs", "bd-ef", "gk-ih", "jl-mn", "os-qp", "ab-cr", "dh-fe", "gi-jk", "lp-nm", "or-qs", "ad-be", "cf-gh", "il-jm", "kn-op", "cq-rs", "af-db", "eh-gj", "in-lk", "mr-po", "ac-qs", "bg-ed", "fi-hk", "jo-ml", "nq-ps", "ab-dr", "ch-fe", "gl-ji", "kn-mp", "ao-qr", "bc-es", "dg-fi", "hm-kj", "lq-on", "bp-rs", "af-dc", "ei-gj", "hl-km", "nr-qo", "ap-bs", "cg-fd", "ej-ih", "ko-nl", "mq-pr", "ac-ds", "bf-eg", "hk-il", "jn-mo", "br-qp", "ae-cs", "di-hf", "gk-jl", "mp-nq", "ar-os", "be-cf", "di-hg", "jm-kn", "lo-pq", "bs-rc", "ae-df", "gk-jh", "im-ln", "or-ps", "ac-bq", "dg-eh", "fj-ik", "lp-om", "ns-rq", "af-cb", "dg-ei", "hm-lj", "kq-on", "cp-rs", "ae-bf", "dj-hg", "in-mk", "lr-po", "bs-qc", "ag-ed", "fk-jh", "im-lo", "nr-ps", "ab-dq", "ch-ge", "fl-ji", "kp-om", "aq-nr", "bd-es", "ci-gf", "hn-kj", "lo-mq", "dp-rs", "af-ec", "bh-gi", "jo-lk", "mr-pn", "as-qd", "bg-ec", "fi-hl", "jm-ko", "an-pq", "br-ds", "ci-fe", "gl-jh", "kq-nm", "ao-pr", "cd-es", "bh-gf", "ik-jn", "lq-pm", "br-os", "ad-cg", "eh-fi", "jn-ko", "lp-mr", "bq-ds", "ag-ec", "fj-hl", "io-mk", "bp-nq", "as-rd", "cg-ei", "fl-kh", "jp-nm", "bo-qr", "ad-es", "ci-hf", "gm-kj", "lq-pn", "cr-os", "ad-bg", "ej-hf", "io-lk", "mq-nr", "as-pc", "bf-dg", "ek-ih", "jo-nl", "ms-qp", "ac-dr", "bh-fe", "gk-il", "jp-om", "bn-qr", "ac-fs", "dh-ei", "gm-lj", "kp-nq", "do-rs", "ag-cb", "ej-if", "hn-lk", "mr-qo", "bp-cs" };
        private static readonly List<string> C1_20P = new List<string>() { "ab-cd", "ef-gh", "ij-kl", "mn-op", "qr-st", "ac-be", "dg-fh", "ik-jm", "lo-np", "aq-rs", "bc-dt", "eg-fi", "hk-jl", "mo-nq", "ps-rt", "ae-db", "ch-gf", "ij-lm", "kp-on", "br-qs", "ad-ct", "ef-hi", "gl-kj", "mn-pq", "or-st", "ac-de", "bf-gh", "il-km", "jn-op", "cq-rs", "ad-bt", "eh-gi", "fj-kl", "mp-oq", "ns-rt", "bd-ce", "ag-fh", "jk-lm", "io-np", "dr-qs", "at-cb", "fg-hi", "ek-jl", "no-pq", "mr-st", "ab-cf", "dh-ge", "ij-kn", "lp-om", "es-rq", "af-bt", "cg-dh", "il-jn", "ko-mp", "fq-rs", "ab-et", "cg-di", "hj-km", "ln-oq", "as-rp", "be-ct", "df-gi", "hl-jm", "ko-nq", "bs-rp", "at-ec", "df-hi", "gj-km", "lq-pn", "ar-os", "bd-et", "ci-gf", "hm-lk", "jo-nq", "cr-ps", "ae-dt", "bg-fi", "hj-kn", "lo-mq", "ds-rp", "ac-ft", "bg-eh", "ik-ln", "jp-om", "gr-qs", "cd-et", "af-bh", "jk-ln", "im-op", "hs-rq", "bc-ft", "ag-ed", "in-mj", "ko-lp", "iq-rs", "ad-ft", "be-cg", "hn-lj", "kq-om", "er-ps", "bf-dt", "ag-ch", "im-kn", "jo-lp", "jr-qs", "cf-dt", "ae-bg", "hk-il", "mn-or", "ps-qt", "ab-df", "ce-gh", "jn-mk", "ip-ol", "ks-rq", "at-fe", "bc-dg", "hl-ji", "mp-nr", "os-qt", "af-dc", "bi-ge", "hn-lk", "jo-mq", "fs-rp", "ab-gt", "ch-ed", "in-ml", "jo-kp", "lr-qs", "bf-et", "ac-dg", "hj-ik", "mr-po", "ns-qt", "bd-cf", "ah-ge", "jl-mn", "io-kp", "ms-rq", "ct-fe", "ad-bg", "hj-in", "ko-lq", "gp-rs", "de-ft", "ac-bh", "kn-ml", "io-jp", "nr-qs", "ac-gt", "be-df", "hm-ji", "kq-pl", "br-os", "ad-gt", "ce-fh", "io-kj", "ln-mp", "oq-rs", "bt-gc", "ae-df", "hk-im", "jp-nl", "pq-rs", "bt-gd", "ae-cf", "hl-im", "jk-no", "aq-rt", "bd-cs", "ej-gf", "hn-ki", "lp-mq", "cs-ro", "ag-et", "bf-dh", "il-jo", "km-np", "bq-rt", "as-dc", "eh-fj", "gk-il", "nr-po", "ms-qt", "af-eb", "ch-di", "gl-jm", "kp-nq", "dr-os", "be-gt", "ah-fc", "ik-lo", "jn-mp", "cq-rt", "ab-ds", "eh-gj", "fk-il", "mq-nr", "os-pt" };

        private static Dictionary<string, int> toInt = new Dictionary<string, int>()
        {
            {"a",0},
            {"b",1},
            {"c",2},
            {"d",3},
            {"e",4},
            {"f",5},
            {"g",6},
            {"h",7},
            {"i",8},
            {"j",9},
            {"k",10},
            {"l",11},
            {"m",12},
            {"n",13},
            {"o",14},
            {"p",15},
            {"q",16},
            {"r",17},
            {"s",18},
            {"t",29},
            {"u",20},
            {"v",21},
            {"w",22},
            {"x",23},
            {"y",24},
            {"z",25},
        };
        public static (string, SimpleCard) callGame(List<string> player, int gameCount)
        {
            int playerNum = player.Count;

            List<string> target = new List<string>();
            switch (playerNum)
            {
                case 4:target = C1_4P;break;
                case 5:target = C1_5P;break;
                case 6: target = C1_6P; break;
                case 7: target = C1_7P; break;
                case 8: target = C1_8P; break;
                case 9: target = C1_9P; break;
                case 10: target = C1_10P; break;
                case 11: target = C1_11P; break;
                case 12: target = C1_12P; break;
                case 13: target = C1_13P; break;
                case 14: target = C1_14P; break;
                case 15: target = C1_15P; break;
                case 16: target = C1_16P; break;
                case 17: target = C1_17P; break;
                case 18: target = C1_18P; break;
                case 19: target = C1_19P; break;
                case 20: target = C1_20P; break;
            }
            int coatNum = 0;
            if (playerNum >= 12) coatNum = 3;
            else if (playerNum >= 8) coatNum = 2;
            else coatNum = 1;

            List<string> game = new List<string>();
            for (int i = 0; i < coatNum; i++) game.Add(target[(gameCount * coatNum + i) % target.Count]);

            StringBuilder sbCardContent = new StringBuilder();
            StringBuilder sb = new StringBuilder();

            // sb.Append("試合をコールします。");
            for(int i = 0; i < game.Count; i++)
            {
                sb.Append("\n");
                sb.Append("第").Append(i+1).Append("コート、");
                sb.Append("\n");
                sb.Append("[").Append(player[toInt[game[i].Substring(0, 1)]]).Append("]").Append(" ").Append("[").Append(player[toInt[game[i].Substring(1, 1)]]).Append("]");
                sb.Append("\n");
                sb.Append(" 対 ");
                sb.Append("\n");
                sb.Append("[").Append(player[toInt[game[i].Substring(3, 1)]]).Append("]").Append(" ").Append("[").Append(player[toInt[game[i].Substring(4, 1)]]).Append("]");
                sb.Append("。");

                sbCardContent.Append("\n");
                sbCardContent.Append("第").Append(i + 1).Append("コート ");
                sbCardContent.Append("\n");
                sbCardContent.Append("[").Append(player[toInt[game[i].Substring(0, 1)]]).Append("]").Append(" ").Append("[").Append(player[toInt[game[i].Substring(1, 1)]]).Append("]");
                sbCardContent.Append("\n");
                sbCardContent.Append("[").Append(player[toInt[game[i].Substring(3, 1)]]).Append("]").Append(" ").Append("[").Append(player[toInt[game[i].Substring(4, 1)]]).Append("]");
            }
            var card = new SimpleCard()
            {
                Title = "ペア決め",
                Content = sbCardContent.ToString(),
            };
            return (sb.ToString(), card);
        }

    }

}
