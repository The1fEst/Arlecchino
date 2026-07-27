import type * as Preset from '@docusaurus/preset-classic';
import type {Config} from '@docusaurus/types';
import {themes as prismThemes} from 'prism-react-renderer';

const config: Config = {
  title: 'Arlecchino',
  tagline: 'A terminal UI framework for .NET',
  favicon: 'img/arlecchino-icon-64.png',

  url: 'https://the1fest.github.io',
  baseUrl: '/Arlecchino.Docs/',
  organizationName: 'The1fEst',
  projectName: 'Arlecchino.Docs',
  trailingSlash: false,

  onBrokenLinks: 'throw',
  onBrokenAnchors: 'throw',
  onDuplicateRoutes: 'throw',

  future: {
    faster: true,
    v4: {
      removeLegacyPostBuildHeadAttribute: true,
      useCssCascadeLayers: false,
    },
  },

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  markdown: {
    format: 'md',
    mermaid: true,
    hooks: {
      onBrokenMarkdownLinks: 'throw',
      onBrokenMarkdownImages: 'throw',
    },
  },

  themes: [
    '@docusaurus/theme-mermaid',
    [
      '@easyops-cn/docusaurus-search-local',
      {
        hashed: true,
        indexBlog: false,
        docsRouteBasePath: '/docs',
        highlightSearchTermsOnTargetPage: true,
        searchResultLimits: 12,
        searchBarShortcutHint: false,
      },
    ],
  ],

  presets: [
    [
      'classic',
      {
        docs: {
          routeBasePath: '/docs',
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/The1fEst/Arlecchino.Docs/tree/master/',
          showLastUpdateTime: true,
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'img/arlecchino-social-card.png',
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true,
    },
    docs: {
      sidebar: {
        hideable: true,
        autoCollapseCategories: false,
      },
    },
    navbar: {
      title: 'Arlecchino',
      logo: {
        alt: 'Arlecchino',
        src: 'img/arlecchino-glyph-light.svg',
        srcDark: 'img/arlecchino-glyph-dark.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docs',
          position: 'left',
          label: 'Documentation',
        },
        {
          type: 'docSidebar',
          sidebarId: 'api',
          position: 'left',
          label: 'API',
        },
        {
          href: 'https://www.nuget.org/packages/Arlecchino',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/The1fEst/Arlecchino',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Documentation',
          items: [
            {label: 'Getting started', to: '/docs/getting-started'},
            {label: 'Views and navigation', to: '/docs/views-and-navigation'},
            {label: 'Migrating to 2.0', to: '/docs/migrating-to-2.0'},
            {label: 'API reference', to: '/docs/api'},
          ],
        },
        {
          title: 'Packages',
          items: [
            {label: 'Arlecchino', href: 'https://www.nuget.org/packages/Arlecchino'},
            {label: 'Arlecchino.Core', href: 'https://www.nuget.org/packages/Arlecchino.Core'},
            {label: 'Arlecchino.Testing', href: 'https://www.nuget.org/packages/Arlecchino.Testing'},
          ],
        },
        {
          title: 'Repository',
          items: [
            {label: 'Source', href: 'https://github.com/The1fEst/Arlecchino'},
            {label: 'Changelog', href: 'https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md'},
            {label: 'Issues', href: 'https://github.com/The1fEst/Arlecchino/issues'},
            {label: 'This site', href: 'https://github.com/The1fEst/Arlecchino.Docs'},
          ],
        },
      ],
      copyright: 'Arlecchino is MIT-licensed. Built with Docusaurus.',
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.vsDark,
      additionalLanguages: ['csharp', 'bash', 'powershell', 'json', 'ini', 'diff'],
    },
    mermaid: {
      theme: {light: 'neutral', dark: 'dark'},
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
