module.exports = {
  siteTitle: 'SharpConfig',
  siteUrl: 'https://dervis.de/sharpconfig',

  logo: {
    light: '/assets/images/logo-splash-light.webp',
    dark: '/assets/images/logo-splash-dark.webp',
    alt: 'SharpConfig logo',
    href: '/',
  },

  srcDir: 'docs',
  outputDir: 'site',

  sidebar: {
    collapsible: true,
    defaultCollapsed: false,
  },

  // Theme Configuration
  theme: {
    name: 'sky',
    defaultMode: 'light',
    enableModeToggle: true,
    positionMode: 'top',
    codeHighlight: true,   
    customCss: [            
      '/assets/css/custom.css',
    ]
  },

  autoTitleFromH1: true,
  copyCode: true,

  plugins: {
    seo: {
      defaultDescription: 'SharpConfig is an easy to use cfg/ini configuration library for .NET.',
      openGraph: { 
        defaultImage: '/assets/images/sharpconfig-preview.png',
      },
      twitter: { 
        cardType: 'summary_large_image',
      }
    },
    sitemap: {
      defaultChangefreq: 'weekly',
      defaultPriority: 0.8
    }
  },

  pageNavigation: false,

  navigation: [
      {
          title: 'Home',
          path: '/',
          icon: 'home',
          children: [
              { title: 'Examples', path: '#examples', icon: 'code' },
              { title: 'Options', path: '#options', icon: 'circle-ellipsis' }
          ]
      },
      { title: 'GitHub', path: 'https://github.com/cdervis/SharpConfig', icon: 'github', external: true },
      { title: 'License', path: 'license', icon: 'copyright' }
    ],

  footer: '© ' + new Date().getFullYear() + ' <a href="https://dervis.de">Cem Dervis</a>',
  madeWithDocmd: 'Made with',

  favicon: '/assets/images/favicon.ico',
};
