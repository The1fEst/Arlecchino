import Link from '@docusaurus/Link';
import useBaseUrl from '@docusaurus/useBaseUrl';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import CodeBlock from '@theme/CodeBlock';
import Layout from '@theme/Layout';
import type {ReactNode} from 'react';

import styles from './index.module.css';

const shortestApp = `using MyApp.Navigation;   // where the generator puts ViewKind and AddGeneratedViews

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options => options.MinimumWidth = 60)
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .StartAt(ViewKind.Default);

await builder.Build().RunAsync();`;

const firstView = `public class DefaultView : IArlecchinoView
{
    private readonly Surface _surface;

    public DefaultView(Surface surface) => _surface = surface;

    public void Draw()
    {
        _surface.AppendLine("hello", Theme.Header, Align.Center);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.A ? ViewKind.About : ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("a", "about")];
}`;

type Feature = {
  title: string;
  body: ReactNode;
  to: string;
};

const features: Feature[] = [
  {
    title: 'Views are plain classes',
    body: (
      <>
        A view implements three members and takes its dependencies through the constructor.
        Navigation keeps a history, so going back is free.
      </>
    ),
    to: '/docs/views-and-navigation',
  },
  {
    title: 'Routes that read like an enum',
    body: (
      <>
        A source generator finds every view, command, store and widget in the project and writes{' '}
        <code>ViewKind</code> and the registration extensions for them.
      </>
    ),
    to: '/docs/source-generator',
  },
  {
    title: 'One surface, two layouts',
    body: (
      <>
        <code>Surface</code> is a double-buffered cell grid. Write down the screen with the flow
        API, or place things by coordinate on the canvas.
      </>
    ),
    to: '/docs/rendering',
  },
  {
    title: 'State that redraws itself',
    body: (
      <>
        Atoms notify on change, computed values follow them, tracked atoms undo, and async atoms
        load on a background thread with a spinner.
      </>
    ),
    to: '/docs/atoms',
  },
  {
    title: 'Modals, a palette and a picker',
    body: (
      <>
        Text, password, number, slider, toggle, choice, multi-choice, date, time and colour modals
        come with the framework, along with a command palette and a file picker.
      </>
    ),
    to: '/docs/modals',
  },
  {
    title: 'Nothing hardcoded in English',
    body: (
      <>
        Every string the framework draws is a delegate an application can point at its own
        translations, including the ones inside the widgets.
      </>
    ),
    to: '/docs/localization',
  },
  {
    title: 'Tested headlessly',
    body: (
      <>
        <code>ArlecchinoTestHost</code> runs an application against a fake terminal and hands back
        the frame as plain text, so assertions are about what was drawn.
      </>
    ),
    to: '/docs/testing',
  },
  {
    title: 'Ready for native AOT',
    body: (
      <>
        The packages are annotated for trimming and published with <code>IsAotCompatible</code>, and
        CI draws a frame from a native build on every push.
      </>
    ),
    to: '/docs/packages-and-building',
  },
];

function Hero(): ReactNode {
  return (
    <header className={styles.hero}>
      <div className="container">
        <img
          className={styles.banner}
          src={useBaseUrl('img/arlecchino-banner.svg')}
          alt="Arlecchino"
        />
        <p className={styles.tagline}>
          A terminal UI framework for .NET. Views are plain classes, navigation keeps a history, and
          everything is wired through <code>Microsoft.Extensions.DependencyInjection</code>.
        </p>
        <div className={styles.install}>
          <CodeBlock language="bash">dotnet add package Arlecchino</CodeBlock>
        </div>
        <div className={styles.buttons}>
          <Link className="button button--primary button--lg" to="/docs/getting-started">
            Get started
          </Link>
          <Link className="button button--secondary button--lg" to="/docs">
            Read the docs
          </Link>
          <Link className="button button--secondary button--lg" to="/docs/api">
            API reference
          </Link>
        </div>
      </div>
    </header>
  );
}

function Features(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {features.map((feature) => (
            <div className="col col--3" key={feature.title}>
              <Link className={styles.feature} to={feature.to}>
                <h3>{feature.title}</h3>
                <p>{feature.body}</p>
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function Sample(): ReactNode {
  return (
    <section className={styles.sample}>
      <div className="container">
        <div className="row">
          <div className="col col--6">
            <h2>The shortest application</h2>
            <CodeBlock language="csharp">{shortestApp}</CodeBlock>
          </div>
          <div className="col col--6">
            <h2>The first view</h2>
            <CodeBlock language="csharp">{firstView}</CodeBlock>
          </div>
        </div>
      </div>
    </section>
  );
}

function Screenshot(): ReactNode {
  return (
    <section className={styles.screenshot}>
      <div className="container">
        <h2>What it looks like</h2>
        <p>
          <Link to="/docs/showcase">Arlecchino.Packages</Link> is a dependency review of a .NET
          solution built on the framework: a sortable table, tabs, a tree, a form on atoms, every
          modal, the command palette and the file picker.
        </p>
        <img
          src={useBaseUrl('img/screenshots/inventory.png')}
          alt="Every package in the solution, coloured by what is wrong with it"
        />
      </div>
    </section>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();

  return (
    <Layout title={siteConfig.title} description={siteConfig.tagline}>
      <Hero />
      <main>
        <Features />
        <Sample />
        <Screenshot />
      </main>
    </Layout>
  );
}
