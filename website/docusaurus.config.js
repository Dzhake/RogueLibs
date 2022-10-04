/** @type {import('@docusaurus/types').DocusaurusConfig} */
module.exports = {
  title: 'RogueLibs Documentation',
  tagline: 'Doing the impossible.',
  url: 'https://sugarbarrel.github.io',
  baseUrl: '/RogueLibs/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'SugarBarrel',
  projectName: 'RogueLibs',
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ru'],
    localeConfigs: {
      en: {
        label: 'English',
      },
      ru: {
        label: 'Русский',
      },
    },
  },
  plugins: ['docusaurus-plugin-sass'],
  themeConfig: {
    metadata: [
      { name: "twitter:image", content: "/RogueLibs/img/logo.png" },
    ],
    docs: {
      sidebar: {
        hideable: true,
      },
    },
    prism: {
      theme: require('prism-react-renderer/themes/dracula'),
      additionalLanguages: ['clike', 'csharp', 'bash'],
    },
    announcementBar: {
      id: 'star',
      content:
        '<span style="font-size: 1rem;">⭐️ If you like RogueLibs, give it a star on <a target="_blank" href="https://github.com/SugarBarrel/RogueLibs">GitHub</a>! ⭐️</span>',
    },
    navbar: {
      hideOnScroll: true,
      title: '',
      logo: {
        alt: 'RogueLibs Logo',
        src: 'img/logo-long.png',
      },
      items: [
        {
          to: 'docs/user/installation',
          label: 'Installation',
          position: 'left',
        },
        {
          to: 'docs/dev/getting-started',
          label: 'Documentation',
          position: 'left',
        },
        {
          to: 'docs/site/intro',
          label: 'Components',
          position: 'left',
        },
        {
          to: 'blog',
          label: 'Blog',
          position: 'left'
        },
        {
          type: 'localeDropdown',
          position: 'right',
        },
        {
          href: 'https://github.com/SugarBarrel/RogueLibs',
          position: 'right',
          className: 'header-github-link',
          'aria-label': 'GitHub repository',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Introduction',
              to: 'docs/intro',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Streets of Rogue Discord',
              href: 'https://discord.com/invite/streetsofrogue',
            }
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/SugarBarrel/RogueLibs',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} RogueLibs. Built with Docusaurus.`,
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl:
            'https://github.com/SugarBarrel/RogueLibs/edit/main/website/',
        },
        blog: {
          showReadingTime: true,
          editUrl:
            'https://github.com/SugarBarrel/RogueLibs/edit/main/website/blog/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
};
